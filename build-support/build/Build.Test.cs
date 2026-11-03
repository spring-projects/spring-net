using System.Collections.Generic;
using System.Linq;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Solutions;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

public partial class Build
{
    [Parameter] readonly bool TestFull = false;
    [Parameter] readonly bool TestIntegrationData = false;
    [Parameter] readonly bool TestIntegrationNms = false;
    [Parameter] readonly bool TestIntegrationMsMq = false;

    AbsolutePath TestResultsDirectory => ArtifactsDirectory / "TestResults";

    // The integration test projects that provision their own SQL Server with Testcontainers (test/Shared/SqlServerTestEnvironment.cs).
    // They need a running Docker daemon, or a server named by the SPRING_TEST_SQLSERVER environment variable, and nothing else.
    static readonly string[] SqlServerIntegrationTestProjects =
    [
        "Spring.Data.Integration.Tests",
        "Spring.Data.NHibernate5.Integration.Tests",
        "Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests",
        "Spring.Scheduling.Quartz3.Integration.Tests",
    ];

    // Projects that target net472 only (no net10.0 build to run on Linux).
    static readonly string[] WindowsOnlyIntegrationTestProjects =
    [
        "Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests",
    ];

    static bool IsSqlServerIntegrationTestProject(Project project) => SqlServerIntegrationTestProjects.Contains(project.Name);

    Target Test => _ => _
        .DependsOn(Restore)
        .After(Compile)
        .Executes(() =>
        {
            var testTargets = GetActiveProjects()
                // The test projects only: Spring.Testing.NUnit and Spring.Testing.Microsoft are shipped libraries that
                // merely contain ".Test", and a Microsoft.Testing.Platform run of a project without tests is an error.
                .Where(x => x.Name.EndsWith(".Tests"))
                // Integration tests need infrastructure, so they run only when asked for: --test-full for all of them, or one
                // flag for the projects behind it. (The four SQL Server projects also have the IntegrationTest target.)
                .Where(x => !x.Name.Contains(".Integration")
                            || TestFull
                            || (TestIntegrationData && IsSqlServerIntegrationTestProject(x))
                            || (TestIntegrationNms && x.Name == "Spring.Messaging.Nms.Integration.Tests"))
                .Where(x => !x.Name.Contains("Spring.Web.Conversation.NHibernate5.Tests") || TestFull)
                .Where(x => !x.Name.Contains("Spring.Messaging.Tests") || TestIntegrationMsMq || TestFull)
                .Where(x => !x.Name.Contains("Spring.Testing.Microsoft.Test") || EnvironmentInfo.IsWin)
                .Where(x => !x.Name.Contains("Spring.Web.Tests") || EnvironmentInfo.IsWin)
                .Where(x => !x.Name.Contains("Spring.Web.Mvc5.Tests") || EnvironmentInfo.IsWin)
                .Where(x => !WindowsOnlyIntegrationTestProjects.Contains(x.Name) || EnvironmentInfo.IsWin);

            RunTestProjects(testTargets);
        });

    // Runs the four SQL Server backed integration test projects. Each starts its own SQL Server container, so a Docker
    // daemon must be running (or SPRING_TEST_SQLSERVER must name a server). Unlike CI-only targets this one is meant to be
    // run locally: ./build.cmd IntegrationTest
    Target IntegrationTest => _ => _
        .DependsOn(Restore)
        .After(Compile)
        .Executes(() =>
        {
            var testTargets = GetActiveProjects()
                .Where(IsSqlServerIntegrationTestProject)
                .Where(x => !WindowsOnlyIntegrationTestProjects.Contains(x.Name) || EnvironmentInfo.IsWin);

            RunTestProjects(testTargets);
        });

    void RunTestProjects(IEnumerable<Project> testTargets)
    {
        var failedProjects = new List<string>();
        foreach (var project in testTargets)
        {
            // The repo runs tests on Microsoft.Testing.Platform (global.json), where the project goes in --project
            // rather than as a positional argument, which DotNetTest's settings cannot express. Each run also
            // writes a TRX file under artifacts/TestResults/<project>, so a red CI log has something to open.
            // The interpolated string has to go to DotNet() as is: its handler quotes each {hole}, a pre-built string
            // would be passed as a single argument.
            var results = TestResultsDirectory / project.Name;
            try
            {
                if (EnvironmentInfo.IsWin)
                {
                    DotNet($"test --project {project.Path} --configuration {Configuration} --no-restore --no-progress --report-trx --results-directory {results}");
                }
                else
                {
                    DotNet($"test --project {project.Path} --configuration {Configuration} --no-restore --no-progress --report-trx --results-directory {results} --framework net10.0");
                }
            }
            catch (ProcessException)
            {
                // Keep going so one red project does not hide the state of the others; the target still fails below.
                failedProjects.Add(project.Name);
            }
        }

        if (failedProjects.Count > 0)
        {
            throw new System.Exception("Test projects failed: " + string.Join(", ", failedProjects));
        }
    }
}
