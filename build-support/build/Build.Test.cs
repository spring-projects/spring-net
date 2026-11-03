using System.Linq;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

public partial class Build
{
    [Parameter] readonly bool TestFull = false;
    [Parameter] readonly bool TestIntegrationData = false;
    [Parameter] readonly bool TestIntegrationNms = false;
    [Parameter] readonly bool TestIntegrationMsMq = false;

    AbsolutePath TestResultsDirectory => ArtifactsDirectory / "TestResults";

    Target Test => _ => _
        .DependsOn(Restore)
        .After(Compile)
        .Executes(() =>
        {
            var testTargets = GetActiveProjects()
                // The test projects only: Spring.Testing.NUnit and Spring.Testing.Microsoft are shipped libraries that
                // merely contain ".Test", and a Microsoft.Testing.Platform run of a project without tests is an error.
                .Where(x => x.Name.EndsWith(".Tests"))
                .Where(x => !x.Name.Contains(".Integration") || TestFull)
                .Where(x => !x.Name.Contains("Spring.Web.Conversation.NHibernate5.Tests") || TestFull)
                .Where(x => !x.Name.Contains("Spring.Data.Integration.Tests") || TestIntegrationData)
                .Where(x => !x.Name.Contains("Spring.Messaging.Tests") || TestIntegrationMsMq || TestFull)
                .Where(x => !x.Name.Contains("Spring.Nms.Integration.Tests") || TestIntegrationNms || TestFull)
                .Where(x => !x.Name.Contains("Spring.Testing.Microsoft.Test") || EnvironmentInfo.IsWin)
                .Where(x => !x.Name.Contains("Spring.Web.Tests") || EnvironmentInfo.IsWin)
                .Where(x => !x.Name.Contains("Spring.Web.Mvc5.Tests") || EnvironmentInfo.IsWin);

            var failedProjects = new System.Collections.Generic.List<string>();
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
        });
}
