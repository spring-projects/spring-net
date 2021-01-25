using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Git;

public partial class Build
{
    string PublicNuGetSource => "https://api.nuget.org/v3/index.json";

    string GitHubRegistrySource => GitHubActions != null
        ? $"https://nuget.pkg.github.com/{GitHubActions.GitHubRepositoryOwner}/index.json"
        : null;

    [Parameter] [Secret] readonly string PublicNuGetApiKey;
    [Parameter] [Secret] readonly string GitHubRegistryApiKey;
    
    /*
    Target IPublish.Publish => _ => _
        .Inherit<IPublish>()
        .Consumes(From<IPack>().Pack)
        .Requires(() => IsOriginalRepository && AppVeyor != null && (GitRepository.IsOnMasterBranch() || GitRepository.IsOnReleaseBranch()) ||
                        !IsOriginalRepository)
        .WhenSkipped(DependencyBehavior.Execute);
        
        */
}