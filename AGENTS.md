# AGENTS.md

Guidance for AI coding agents working in this repository. Human-facing documentation is in README.md.

## Project overview

Spring.NET is a port and extension of the Java Spring Framework for .NET: IoC container, AOP, expression language, declarative transaction management, ADO.NET framework, ASP.NET (WebForms/MVC5/WebAPI) integration, NHibernate 5, Quartz.NET, messaging (MSMQ, NMS, TIBCO EMS), NVelocity templating, and NUnit/MSTest testing support. Apache 2.0 licensed. Packages are published to NuGet from git tags (`v*.*.*`) via NuGet Trusted Publishing (GitHub OIDC), not a stored API key.

## Build and test commands

Build orchestration is Fallout (`build-support/build`), the maintained successor to NUKE. Root-level bootstrap scripts install the pinned .NET SDK if missing (`global.json`: 10.0.100, rollForward latestMinor), then run the build through the `fallout` local tool (`dotnet tool restore` + `dotnet fallout`).

```
./build.cmd            # Windows (build.sh on Linux/macOS); default target = Compile
./build.cmd Compile    # build Spring.Net.sln (+ examples, Windows only)
./build.cmd Test       # run the default test suite
./build.cmd Ci         # Compile + Test + Pack — what CI runs
./build.cmd Clean
./build.cmd Antlr      # regenerate the expression-language parser (Windows only)
```

Plain `dotnet` CLI works too — the solution is `Spring.Net.sln`:

```
dotnet build Spring.Net.sln
dotnet test test/Spring/Spring.Core.Tests/Spring.Core.Tests.csproj
dotnet test test/Spring/Spring.Core.Tests/Spring.Core.Tests.csproj -f net8.0 --filter "FullyQualifiedName~XmlObjectFactoryTests"
```

- Test framework is **NUnit 3** (`Spring.Testing.Microsoft.Tests` alone uses MSTest); mocking uses **FakeItEasy**.
- CI (`.github/workflows/ci.yml`) runs `./build.cmd ci` on windows-latest and `./build.sh ci` on ubuntu-latest for every PR to `main`.
- **Warnings are errors** in `src/` and `test/` (`TreatWarningsAsErrors=True`), and formatting rule IDE0055 is warning severity — a formatting violation fails the build.
- Build output goes to `build/$(Configuration)/$(ProjectName)/` (not in-place `bin/`); NuGet packages to `artifacts/`.

### Integration tests

Fallout's `Test` target excludes `*.Integration.Tests` projects (and `Spring.Web.Conversation.NHibernate5.Tests`) by default; they need external infrastructure and are gated behind flags: `--test-full`, `--test-integration-data`, `--test-integration-nms`, `--test-integration-ms-mq`, `--test-integration-ems`. `Spring.Services.Tests` is always excluded. Several test projects are Windows-only (net462: Web, Mvc5, Velocity, Messaging/MSMQ).

- SQL Server-backed tests (`Spring.Data.Integration.Tests`, `Spring.Data.NHibernate5.Integration.Tests`, `Spring.Scheduling.Quartz3.Integration.Tests`): `docker-compose.yml` brings up SQL Server; setup scripts in `build-support/*.sql` and the test projects' `Data/` folders. See README.md "Running tests" for the full recipe.
- TIBCO EMS projects need `TIBCO.EMS.dll` (not in the repo) and are excluded from the solution build; enable with `--build-ems`.

## Architecture

`src/Spring/` has one directory per shipped assembly. All projects use `RootNamespace=Spring` and source folders mirror namespaces. Dependency spine: **Spring.Core → Spring.Aop → Spring.Data → everything else**.

| Project | Purpose |
|---|---|
| Spring.Core | IoC container, expression language, validation, dynamic reflection |
| Spring.Aop | AOP framework (dynamic proxies, advice, pointcuts) |
| Spring.Data | Transaction management + ADO.NET framework |
| Spring.Data.NHibernate5 | NHibernate 5 integration (RootNamespace `Spring.Data.NHibernate`) |
| Spring.Web / .Extensions / .Mvc5 | ASP.NET WebForms / AJAX / MVC5+WebAPI integration (net462 only) |
| Spring.Services | Remoting, web services, Enterprise Services (net462 only) |
| Spring.Messaging / .Nms / .Ems | MSMQ / Apache NMS / TIBCO EMS integration |
| Spring.Scheduling.Quartz3 | Quartz.NET 3.x integration |
| Spring.Template.Velocity(.Castle) | NVelocity templating |
| Spring.Testing.NUnit / .Microsoft | DI-enabled test fixture base classes |

Key mechanics (defined in `src/Directory.Build.props` and `test/Directory.Build.props` — there is **no root** Directory.Build.props):

- **Multi-targeting**: most libraries target `netstandard2.0;net462`; web/services/EMS projects are net462-only. Projects reference the shared MSBuild variable `$(TargetFullFrameworkVersion)` (= net462) instead of literal TFMs. Framework differences are handled with conditional `<Compile Remove>`/`<ItemGroup>` blocks in csproj files, not scattered `#if` directives.
- **Strong naming**: all assemblies signed with the committed `Spring.Net.snk`.
- **Central package management**: versions live in `Directory.Packages.props`; csproj `PackageReference`s are versionless.
- **ANTLR expression parser**: the grammar is `src/Spring/Spring.Core/Expressions/Expression.g`. The generated lexer/parser under `Expressions/Parser/` and a vendored, hand-patched ANTLR 2.7.7 runtime under `Expressions/Parser/antlr/` are **committed — never hand-edit generated files**; regenerate with `./build.cmd Antlr` (see `src/Spring/Spring.Core/README_ANTLR.txt`).
- **Versioning**: the git tag is the version authority (`v3.1.0` → 3.1.0); untagged builds get a `dev-`/`preview-` suffix. Publishing runs from the tag workflow only.
- **Trusted publishing**: `.github/workflows/publish.yml` is the only workflow allowed to push to nuget.org. It needs `permissions: id-token: write` and `environment: nuget`; the `Publish` target exchanges that OIDC token for a short-lived API key in `build-support/build/Build.Publish.cs`. The nuget.org policy is keyed to the **workflow filename** `publish.yml` plus the `nuget` environment, so renaming, moving or splitting that workflow breaks publishing until the policy is updated. Do not reintroduce a `NUGET_API_KEY` secret — the `NuGetApiKey` parameter stays only as a manual override.
- In test projects, `ILog` is a global using alias for `Microsoft.Extensions.Logging.ILogger`.

## Code style

`.editorconfig` is authoritative: 4-space indents (2 for XML/props/ps1), Allman braces, `System` usings first, no `this.` qualification, language keywords over BCL type names, instance fields `_camelCase`, static fields `s_camelCase`. Match the surrounding code's style. (The `[src/{Analyzers,...}/**]` sections were copied from the Roslyn repo and match nothing here — ignore them.)

**Logging**: all `ILogger` calls use constant message templates with PascalCase named placeholders (`log.LogDebug("Creating object '{ObjectName}'", name)`) — never interpolation, concatenation, or `string.Format`. Pass exceptions to the exception-first overloads; wrap inherently dynamic text as `Log*("{Message}", text)`. Analyzers CA2254/CA2253/CA2017/CA1727 enforce this as build errors. Only guard a log call with `IsEnabled` when argument evaluation is expensive (string building, method calls) — templates already defer formatting.

## Legacy areas — do not modify

- NAnt-era build files, superseded by the Fallout build but still committed: `Spring.build`, `*.include`, `Build-ci.cmd`, `build-release-all.cmd`, `appveyor.yml.old`, most of `build-support/` (exception: `build-support/tools/antlr-2.7.6` is still used by the `Antlr` target).
- `dev-support/`, `templates/` — VS 2008-era templates. `lib/` — checked-in legacy third-party DLLs. `doc/` — DocBook sources + vendored toolchain (CI ignores `doc/**`).
- Orphan test folders not in the solution (reference removed projects): `test/Spring/Spring.Web.Mvc.Tests`, `Spring.Messaging.Ems.Tests`, `Spring.Data.NHibernate.Tests`.

## Website

`website/` is a Gatsby 5 + React + TypeScript site (package manager: **yarn**). Local dev: `yarn install && yarn start`. The full build is `./build.sh website` (Fallout target; also builds the DocBook reference docs, which require Java on PATH). Deployed to GitHub Pages by `.github/workflows/website.yml` on pushes to `main`.
