using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Fallout.Common;
using Fallout.Common.CI;
using Fallout.Common.CI.AppVeyor;
using Fallout.Common.CI.GitHubActions;
using Fallout.Common.Git;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Tools.MSBuild;
using Fallout.Common.Utilities.Collections;
using Fallout.Solutions;
using Serilog;

using static Fallout.Common.Tooling.ProcessTasks;
using static Fallout.Common.Tools.DotNet.DotNetTasks;
using static Fallout.Common.Tools.MSBuild.MSBuildTasks;

[ShutdownDotNetAfterServerBuild]
partial class Build : FalloutBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Build EMS")] readonly bool BuildEms = false;

    [Parameter("Version")] readonly string ProjectVersion = "3.1.0";

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    [CI] readonly AppVeyor AppVeyor;
    [CI] readonly GitHubActions GitHubActions;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "test";
    AbsolutePath BuildDirectory => RootDirectory / "build";
    AbsolutePath ExamplesDirectory => RootDirectory / "examples";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    string TagVersion => GitRepository?.Tags.SingleOrDefault(x => x.StartsWith("v"))?[1..];

    bool IsTaggedBuild => !string.IsNullOrWhiteSpace(TagVersion);

    /// <summary>Numeric part of the version, e.g. <c>3.1.0</c>. Also the file version.</summary>
    string VersionPrefix;

    /// <summary>Prerelease part, e.g. <c>rc.1</c> or <c>preview-20260809-1231</c>; empty for releases.</summary>
    string VersionSuffix;

    /// <summary>Binding identity of the strong-named assemblies, e.g. <c>3.1.0.0</c>.</summary>
    string AssemblyVersion;

    string FullVersion => string.IsNullOrWhiteSpace(VersionSuffix) ? VersionPrefix : $"{VersionPrefix}-{VersionSuffix}";

    static bool IsRunningOnWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    protected override void OnBuildInitialized()
    {
        DetermineVersion();

        Log.Information("BUILD SETUP");
        Log.Information("Configuration:\t{Configuration}", Configuration);
        Log.Information("Version:\t{FullVersion}", FullVersion);
        Log.Information("Assembly version:\t{AssemblyVersion}", AssemblyVersion);
        Log.Information("Tagged build:\t{IsTaggedBuild}", IsTaggedBuild);
    }

    /// <summary>
    /// The git tag is the version authority; untagged builds carry the <see cref="ProjectVersion"/>
    /// placeholder plus a preview/dev suffix so they can never be mistaken for a release.
    /// </summary>
    void DetermineVersion()
    {
        var tagVersion = TagVersion;
        if (!string.IsNullOrWhiteSpace(tagVersion))
        {
            // A prerelease tag (v3.1.0-rc.1) has to be split - only the numeric part is a valid
            // AssemblyVersion, and the remainder belongs in the package version suffix.
            var separator = tagVersion.IndexOf('-');
            VersionPrefix = separator < 0 ? tagVersion : tagVersion[..separator];
            VersionSuffix = separator < 0 ? null : tagVersion[(separator + 1)..];
        }
        else
        {
            VersionPrefix = ProjectVersion;
            VersionSuffix = $"preview-{DateTime.UtcNow:yyyyMMdd-HHmm}";
        }

        if (IsLocalBuild)
        {
            VersionSuffix = $"dev-{DateTime.UtcNow:yyyyMMdd-HHmm}";
        }

        // The assemblies are strong-named, so AssemblyVersion is part of their binding identity and
        // every change to it forces consumers to update binding redirects. Pin it to major.minor so
        // patch releases stay drop-in replacements; FileVersion carries the exact version instead.
        var numericVersion = Version.Parse(VersionPrefix);
        AssemblyVersion = $"{numericVersion.Major}.{numericVersion.Minor}.0.0";
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
                .Apply(CompileVersionSettings)
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
                    .Apply(ExampleVersionSettings)
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

            foreach (var project in packTargets)
            {
                DotNetPack(s => s
                    .SetProject(project.Path)
                    .Apply(PackVersionSettings)
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

            var moduleNames = new[] { "Common.Logging", "Common.Logging.Core", "Spring.Core", "Spring.Aop", "Spring.Data", "Spring.Data.NHibernate*", "Spring.Web", "Spring.Web.Mvc5", "Spring.Web.Extensions", "Spring.Services", "Spring.Testing.NUnit", "Spring.Testing.Microsoft", "Spring.Messaging.Ems", "Spring.Messaging.Nms", "Spring.Messaging", "Spring.Scheduling.Quartz3", "Spring.Template.Velocity", "Spring.Web.Conversation.NHibernate5", };

            var patterns = moduleNames
                .SelectMany(x => new[] { "**/" + Configuration + "/**/" + x + ".dll", "**/" + Configuration + "/**/" + x + ".xml", "**/" + Configuration + "/**/" + x + ".pdb" })
                .ToArray();

            foreach (var file in BuildDirectory.GlobFiles(patterns))
            {
                file.CopyToDirectory(binDirectory / "net", ExistsPolicy.FileOverwriteIfNewer);
            }
        });

    // Compile and Pack have to agree: the packaged assemblies and the ones copied into bin/ by
    // PackBinaries come from different targets, and only Pack used to set any version at all.
    // DotNet*Settings share no base that carries these extensions, hence the two near-identical
    // configurations.
    Configure<DotNetBuildSettings> CompileVersionSettings => _ => _
        .SetAssemblyVersion(AssemblyVersion)
        .SetFileVersion(VersionPrefix)
        .SetVersionPrefix(VersionPrefix)
        .SetVersionSuffix(VersionSuffix);

    Configure<DotNetPackSettings> PackVersionSettings => _ => _
        .SetAssemblyVersion(AssemblyVersion)
        .SetFileVersion(VersionPrefix)
        .SetVersionPrefix(VersionPrefix)
        .SetVersionSuffix(VersionSuffix);

    // Several example solutions include the src/Spring projects directly (Spring.Core.csproj and
    // friends), so this MSBuild call rebuilds them straight back into build/$(Configuration)/.
    // Without the same versions, that rebuild stamps the SDK default 1.0.0.0 over what
    // CompileSolution produced, and the next example to reference two Spring assemblies fails
    // CS1705 on the mismatch. MSBuildSettings has no VersionPrefix/Suffix setter, hence SetProperty.
    Configure<MSBuildSettings> ExampleVersionSettings => _ => _
        .SetAssemblyVersion(AssemblyVersion)
        .SetFileVersion(VersionPrefix)
        .SetProperty("VersionPrefix", VersionPrefix)
        .SetProperty("VersionSuffix", VersionSuffix ?? string.Empty);

    IEnumerable<Project> GetActiveProjects()
    {
        var packTargets = Solution.AllProjects
            .Where(x => x.Name != "Spring.Messaging.Ems" || BuildEms)
            .Where(x => !x.Name.Contains("_build"));
        return packTargets;
    }
}
