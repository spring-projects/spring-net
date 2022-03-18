using System.Linq;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

public partial class Build
{
    [Parameter]
    readonly bool TestFull = false;
    [Parameter]
    readonly bool TestIntegrationData = false;
    [Parameter]
    readonly bool TestIntegrationEms = false;
    [Parameter]
    readonly bool TestIntegrationNms = false;
    [Parameter]
    readonly bool TestIntegrationMsMq = false;

    Target Test => _ => _
        .DependsOn(Restore)
        .After(Compile)
        .Executes(() =>
        {
            var testTargets = GetActiveProjects()
                .Where(x => x.Name.Contains(".Test"))
                .Where(x => !x.Name.Contains(".Integration") || TestFull)
                .Where(x => !x.Name.Contains("Spring.Web.Conversation.NHibernate5.Tests") || TestFull)
                .Where(x => !x.Name.Contains("Spring.Data.Integration.Tests") || TestIntegrationData)
                .Where(x => !x.Name.Contains("Spring.Messaging.Tests") || TestIntegrationMsMq || TestFull)
                .Where(x => !x.Name.Contains("Spring.Nms.Integration.Tests") || TestIntegrationNms || TestFull)
                .Where(x => !x.Name.Contains("Spring.Messaging.Ems.Integration.Tests") || TestIntegrationEms || TestFull)
                .Where(x => !x.Name.Contains("Spring.Services.Tests"))
                .Where(x => !x.Name.Contains("Spring.Template.Velocity.Castle.Tests") || EnvironmentInfo.IsWin)
                .Where(x => !x.Name.Contains("Spring.Template.Velocity.Tests") || EnvironmentInfo.IsWin)
                .Where(x => !x.Name.Contains("Spring.Testing.Microsoft.Test") || EnvironmentInfo.IsWin)
                .Where(x => !x.Name.Contains("Spring.Web.Tests") || EnvironmentInfo.IsWin)
                .Where(x => !x.Name.Contains("Spring.Web.Mvc5.Tests") || EnvironmentInfo.IsWin);

            foreach (var project in testTargets)
            {
                DotNetTest(s =>
                {
                    s = s
                        .SetProjectFile(project.Path)
                        .SetConfiguration(Configuration)
                        .EnableNoRestore();

                    if (!EnvironmentInfo.IsWin)
                    {
                        s = s.SetFramework("net6.0");
                    }

                    return s;
                });
            }
        });

}