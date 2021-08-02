using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tooling.ProcessTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

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
    AbsolutePath BuildDirectory => RootDirectory / "build";
    AbsolutePath ExamplesDirectory => RootDirectory / "examples";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            ExamplesDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
            EnsureCleanDirectory(BuildDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(CompileSolution, CompileExamples);

    Target CompileSolution => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
            );
        });

    Target CompileExamples => _ => _
        .OnlyWhenStatic(() => EnvironmentInfo.IsWin)
        .DependsOn(CompileSolution, PackBinaries)
        .Executes(() =>
        {
            foreach (var solutionFile in ExamplesDirectory.GlobFiles("**/*.sln"))
            {
                if (solutionFile.ToString().Contains("Spring.EmsQuickStart") && !BuildEms
                    || solutionFile.ToString().Contains("Spring.Examples.Pool")
                    || solutionFile.ToString().Contains("SpringAir")
                    || solutionFile.ToString().Contains("Spring.Web.Extensions.Example")
                    || solutionFile.ToString().Contains("Spring.WebQuickStart"))
                {
                    continue;
                }

                MSBuild(s => s
                    .SetTargets("Restore", "Rebuild")
                    .SetConfiguration(Configuration)
                    .SetTargetPath(solutionFile)
                    .SetNodeReuse(false)
                    .SetVerbosity(MSBuildVerbosity.Minimal)
                );
            }
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

            var version = TagVersion;
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

    Target PackBinaries => _ => _
        .DependsOn(CompileSolution)
        .Executes(() =>
        {
            var binDirectory = RootDirectory / "bin";
            EnsureCleanDirectory(binDirectory);

            var moduleNames = new[]
            {
                "Common.Logging",
                "Common.Logging.Core",
                "Spring.Core",
                "Spring.Aop",
                "Spring.Data",
                "Spring.Data.NHibernate*",
                "Spring.Web",
                "Spring.Web.Mvc5",
                "Spring.Web.Extensions",
                "Spring.Services",
                "Spring.Testing.NUnit",
                "Spring.Testing.Microsoft",
                "Spring.Messaging.Ems",
                "Spring.Messaging.Nms",
                "Spring.Messaging",
                "Spring.Scheduling.Quartz3",
                "Spring.Template.Velocity",
                "Spring.Web.Conversation.NHibernate5",
            };

            var patterns = moduleNames
                .SelectMany(x => new []
                {
                    "**/" + Configuration + "/**/" + x + ".dll",
                    "**/" + Configuration + "/**/" + x + ".xml",
                    "**/" + Configuration + "/**/" + x + ".pdb"
                })
                .ToArray();

            foreach (var file in BuildDirectory.GlobFiles(patterns))
            {
                CopyFileToDirectory(file, binDirectory / "net", FileExistsPolicy.OverwriteIfNewer);
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
