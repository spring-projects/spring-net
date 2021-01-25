using Nuke.Common;
using Nuke.Common.CI.GitHubActions;

[GitHubActions(
    name: "ci",
    //GitHubActionsImage.UbuntuLatest,
    GitHubActionsImage.WindowsLatest,
    AutoGenerate = true,
    OnPushBranches = new[] {"main", "master"},
    OnPullRequestBranches = new[] {"main", "master"},
    InvokedTargets = new[] {nameof(Ci)},
    ImportSecrets = new[] {"TODO"},
    PublishArtifacts = true
)]
public partial class Build
{
    Target Ci => _ => _
        .DependsOn(Test, Pack)
        .Executes();
}