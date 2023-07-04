using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using Serilog;
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
    readonly string ProjectVersion = "3.0.2";

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    [CI] readonly AppVeyor AppVeyor;
    [CI] readonly GitHubActions GitHubActions;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "test";
    AbsolutePath BuildDirectory => RootDirectory / "build";
    AbsolutePath ExamplesDirectory => RootDirectory / "examples";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    string TagVersion => GitRepository.Tags.SingleOrDefault(x => x.StartsWith("v"))?[1..];

    bool IsTaggedBuild => !string.IsNullOrWhiteSpace(TagVersion);

    string VersionSuffix;

    static bool IsRunningOnWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    protected override void OnBuildInitialized()
    {
        VersionSuffix = !IsTaggedBuild
            ? $"preview-{DateTime.UtcNow:yyyyMMdd-HHmm}"
            : "";

        if (IsLocalBuild)
        {
            VersionSuffix = $"dev-{DateTime.UtcNow:yyyyMMdd-HHmm}";
        }

        Log.Information("BUILD SETUP");
        Log.Information("Configuration:\t{Configuration}", Configuration);
        Log.Information("Version suffix:\t{VersionSuffix}", VersionSuffix);
        Log.Information("Tagged build:\t{IsTaggedBuild}", IsTaggedBuild);
    }

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
            ArtifactsDirectory.CreateOrCleanDirectory();
            BuildDirectory.CreateOrCleanDirectory();
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
                .Where(x => !x.Name.EndsWith(".Tests"));

            var version = TagVersion;
            if (string.IsNullOrWhiteSpace(version))
            {
                version = ProjectVersion;
            }

            foreach (var project in packTargets)
            {
                DotNetPack(s => s
                    .SetProject(project.Path)
                    .SetVersion(version)
                    .SetVersionSuffix(VersionSuffix)
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
            binDirectory.CreateOrCleanDirectory();

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
        var packTargets = Solution.AllProjects
            .Where(x => x.Name != "Spring.Messaging.Ems" || BuildEms)
            .Where(x => !x.Name.Contains("_build"));
        return packTargets;
    }
}
