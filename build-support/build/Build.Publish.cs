using System;
using System.Collections.Generic;
using System.Net.Http;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Utilities.Net;
using Serilog;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

public partial class Build
{
    // Trusted publishing (https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing): the
    // audience and token exchange endpoint nuget.org expects, and the nuget.org profile name of the
    // account that created the trusted publishing policy.
    const string NuGetAudience = "https://www.nuget.org";
    const string NuGetTokenServiceUrl = "https://www.nuget.org/api/v2/token";
    const string NuGetUser = "lahma";

    [Parameter] string NuGetSource => "https://api.nuget.org/v3/index.json";

    [Parameter("Explicit nuget.org API key - when omitted, a short-lived key is minted from GitHub OIDC")]
    [Secret]
    readonly string NuGetApiKey;

    Target Publish => _ => _
        .OnlyWhenDynamic(() => IsRunningOnWindows && IsTaggedBuild)
        .DependsOn(Pack)
        .Executes(() =>
        {
            // Minted once per release: nuget.org allows one key per 30 seconds per user, and a single
            // OIDC token mints exactly one key.
            var apiKey = !string.IsNullOrWhiteSpace(NuGetApiKey)
                ? NuGetApiKey
                : GetTrustedPublishingApiKey();

            DotNetNuGetPush(_ => _
                    .SetSource(NuGetSource)
                    .SetApiKey(apiKey)
                    .SetSkipDuplicate(true)
                    .CombineWith(PushPackageFiles, (_, v) => _
                        .SetTargetPath(v)),
                PushDegreeOfParallelism,
                PushCompleteOnFailure);
        });

    IEnumerable<AbsolutePath> PushPackageFiles => ArtifactsDirectory.GlobFiles("*.nupkg");

    bool PushCompleteOnFailure => true;
    int PushDegreeOfParallelism => 5;

    /// <summary>
    /// Exchanges the job's GitHub OIDC token for a short-lived nuget.org API key, so no long-lived
    /// key has to be stored anywhere. Does what <c>NuGet/login@v1</c> does, without taking a
    /// dependency on the marketplace action - and late enough in the build that the key is seconds
    /// old by the time the push runs.
    /// </summary>
    string GetTrustedPublishingApiKey()
    {
        const string missingOidc = "GitHub OIDC is unavailable - the job needs 'permissions: id-token: write'";
        var requestUrl = Assert.NotNullOrWhiteSpace(Environment.GetEnvironmentVariable("ACTIONS_ID_TOKEN_REQUEST_URL"), missingOidc);
        var requestToken = Assert.NotNullOrWhiteSpace(Environment.GetEnvironmentVariable("ACTIONS_ID_TOKEN_REQUEST_TOKEN"), missingOidc);

        using var client = new HttpClient();
        // nuget.org's token endpoint returns HTTP 400 for a request with no User-Agent, and HttpClient
        // sends none by default. Any non-empty value satisfies it.
        client.DefaultRequestHeaders.UserAgent.ParseAdd("spring-net-build/1.0");

        var idToken = client
            .CreateRequest(HttpMethod.Get, $"{requestUrl}&audience={Uri.EscapeDataString(NuGetAudience)}")
            .WithBearerAuthentication(requestToken)
            .GetResponse()
            .AssertSuccessfulStatusCode()
            .GetBodyAsJsonObject().GetAwaiter().GetResult()["value"].GetValue<string>();

        var body = client
            .CreateRequest(HttpMethod.Post, NuGetTokenServiceUrl)
            .WithBearerAuthentication(idToken)
            .WithJsonContent(new { username = NuGetUser, tokenType = "ApiKey" })
            .GetResponse()
            .AssertResponse(x => x.IsSuccessStatusCode
                ? null
                : $"nuget.org token exchange failed ({(int) x.StatusCode}): {x.Content.ReadAsStringAsync().GetAwaiter().GetResult()}. "
                + $"Check that a trusted publishing policy exists for this repository and workflow file, and that '{NuGetUser}' created it.")
            .GetBodyAsJsonObject().GetAwaiter().GetResult();

        // The action reads 'apiKey', the original design document says 'api_key' - accept either.
        var apiKey = (body["apiKey"] ?? body["api_key"]).GetValue<string>();
        GitHubActions?.WriteCommand("add-mask", apiKey);
        Log.Information("Obtained short-lived nuget.org API key, expires {Expires}", body["expires"]?.GetValue<string>());
        return apiKey;
    }
}
