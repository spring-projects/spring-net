using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tooling.ProcessTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Build EMS")]
    readonly bool BuildEms = false;

    [Parameter("Version")]
    readonly string ProjectVersion = "3.0.0";

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    [CI] readonly AppVeyor AppVeyor;
    [CI] readonly GitHubActions GitHubActions;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "test";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
            EnsureCleanDirectory(RootDirectory / "build");
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
            );
        });

    Target Antlr => _ => _
        .Executes(() =>
        {
            var projectDir = Solution.GetProject("Spring.Core");
            var expressionsDir = Path.Combine(projectDir.Directory, "Expressions");
            var antlrExecutable = Path.Combine(Solution.Directory, "build-support/tools/antlr-2.7.6/antlr-2.7.6.exe");
            var process = StartProcess(
                toolPath: antlrExecutable,
                arguments: $"-o {expressionsDir}/Parser {expressionsDir}/Expression.g"
            );
            process.WaitForExit();
        });

    Target Pack => _ => _
        .After(Test)
        .DependsOn(Restore)
        .Executes(() =>
        {
            var packTargets = GetActiveProjects()
                .Where(x => !x.Name.Contains(".Test"));

            var version = GitRepository.Tags.SingleOrDefault(x => x.StartsWith("v"))?[1..];
            var suffix = "";
            if (string.IsNullOrWhiteSpace(version))
            {
                version = ProjectVersion;
                suffix = "develop-" + DateTime.UtcNow.ToString("yyyyMMddHHmm");
            }

            foreach (var project in packTargets)
            {
                DotNetPack(s => s
                    .SetProject(project.Path)
                    .SetVersion(version)
                    .SetVersionSuffix(suffix)
                    .SetConfiguration(Configuration.Release)
                    .EnableNoRestore()
                    .SetOutputDirectory(ArtifactsDirectory)
                );
            }
        });

    IEnumerable<Project> GetActiveProjects()
    {
        var packTargets = Solution.GetProjects("*")
            .Where(x => x.Name != "Spring.Messaging.Ems" || BuildEms)
            .Where(x => !x.Name.Contains("_build"));
        return packTargets;
    }
}
