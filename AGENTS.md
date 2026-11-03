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
dotnet test test/Spring/Spring.Core.Tests/Spring.Core.Tests.csproj -f net10.0 --filter "FullyQualifiedName~XmlObjectFactoryTests"
```

- Test framework is **NUnit 3** (`Spring.Testing.Microsoft.Tests` alone uses MSTest); mocking uses **FakeItEasy**.
- CI (`.github/workflows/ci.yml`) runs `./build.cmd ci` on windows-latest and `./build.sh ci` on ubuntu-latest for every PR to `main`.
- **Warnings are errors** in `src/` and `test/` (`TreatWarningsAsErrors=True`), and formatting rule IDE0055 is warning severity — a formatting violation fails the build.
- Build output goes to `build/$(Configuration)/$(ProjectName)/` (not in-place `bin/`); NuGet packages to `artifacts/`.

### Integration tests

Fallout's `Test` target excludes `*.Integration.Tests` projects (and `Spring.Web.Conversation.NHibernate5.Tests`) by default; they need external infrastructure and are gated behind flags: `--test-full`, `--test-integration-data`, `--test-integration-nms`, `--test-integration-ms-mq`, `--test-integration-ems`. `Spring.Services.Tests` is always excluded. Several test projects are Windows-only (net472: Web, Mvc5, Velocity, Messaging/MSMQ).

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
| Spring.Web / .Extensions / .Mvc5 | ASP.NET WebForms / AJAX / MVC5+WebAPI integration (net472 only) |
| Spring.Services | Remoting, web services, Enterprise Services (net472 only) |
| Spring.Messaging / .Nms / .Ems | MSMQ / Apache NMS / TIBCO EMS integration |
| Spring.Scheduling.Quartz3 | Quartz.NET 3.x integration |
| Spring.Template.Velocity(.Castle) | NVelocity templating |
| Spring.Testing.NUnit / .Microsoft | DI-enabled test fixture base classes |

Key mechanics (defined in `src/Directory.Build.props` and `test/Directory.Build.props` — there is **no root** Directory.Build.props):

- **Multi-targeting**: most libraries target `netstandard2.0;net472`; web/services/EMS projects are net472-only. Projects reference the shared MSBuild variable `$(TargetFullFrameworkVersion)` (= net472) instead of literal TFMs. Framework differences are handled with conditional `<Compile Remove>`/`<ItemGroup>` blocks in csproj files, not scattered `#if` directives.
- **Strong naming**: all assemblies signed with the committed `Spring.Net.snk`.
- **Central package management**: versions live in `Directory.Packages.props`; csproj `PackageReference`s are versionless.
- **ANTLR expression parser**: the grammar is `src/Spring/Spring.Core/Expressions/Expression.g`. The generated lexer/parser under `Expressions/Parser/` and a vendored, hand-patched ANTLR 2.7.7 runtime under `Expressions/Parser/antlr/` are **committed — never hand-edit generated files**; regenerate with `./build.cmd Antlr` (see `src/Spring/Spring.Core/README_ANTLR.txt`).
- **Versioning**: the git tag is the version authority (`v3.1.0` → 3.1.0); untagged builds fall back to the `ProjectVersion` placeholder in `build-support/build/Build.cs` plus a `dev-`/`preview-` suffix. Publishing runs from the tag workflow only. `DetermineVersion()` sets three distinct facets, applied to **both** `Compile` and `Pack` so packaged and `bin/` assemblies agree: `AssemblyVersion` is pinned to `major.minor.0.0` (the assemblies are strong-named, so this is their binding identity — keeping it stable across patches means consumers need no new binding redirect for a fix release), `FileVersion` carries the exact numeric version, and `InformationalVersion` is left to MSBuild so SourceLink can append `+<sha>`. Never set a bare `Version` property instead — it overrides the prefix/suffix composition and silently drops the preview suffix.
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

Gatsby 5 constraints on dependency updates:

- Three transitive advisories can't be fixed while the site is on Gatsby 5. Dismiss them; don't force versions.
  - `sharp` 0.32: Gatsby's plugins call `sharp.simd()`, which was removed in 0.33.
  - `file-type` 16: the fixed version is ESM-only, and `gatsby-source-filesystem` `require()`s it.
  - `@parcel/reporter-dev-server` 2.8.3: parcel core 2.8.3's engine check rejects the fix.
- `gatsby build` on Node ≥ 22.7 needs `msgpackr` ≥ 1.11.2 and `ordered-binary` ≥ 1.6.
- `resolutions` in `website/package.json` hold the express-related transitive packages at patched versions. Update them; don't drop them.

## Agent hazards

These facts have cost real debugging time. They apply to every agent and human working here.

- **Line endings and encodings.** Working trees can be CRLF (`* text=auto`, and files older than the normalization). Never rewrite a tracked file with `sed -i` or another whole-file shell rewrite: the diff turns into every line. Use an editor or the Edit tool. `WebConversationStateTest.cs` is UTF-16, so grep and ripgrep sweeps miss its content.
- **Direct project builds need a configuration.** `dotnet build <csproj>` without `-c Debug` (or `Release`) writes to `build/<Project>/` instead of `build/<Configuration>/<Project>/`, because `OutputPath` is computed before `Configuration` defaults. Tests then run stale DLLs. Solution builds are unaffected.
- **Examples.**
  - `CompileExamples` uses the full-framework MSBuild, which must be 18+ (VS 2026) for the SDK that `global.json` resolves. Without it, run `./build.cmd ci --skip CompileExamples` and check an example with `dotnet build <example>.sln -c Release`.
  - Four example solutions include the `src/Spring` projects directly and rebuild them into `build/$(Configuration)/`. Any MSBuild property that `Build.cs` passes to the compile and pack calls must also go to the examples call.
- **Culture-dependent tests.** `DateTimeFormatterTests` fails on a machine whose regional settings override the en-US formats. That failure isn't a regression.
- **Fallout versions.** `fallout.globaltool` in `.config/dotnet-tools.json` and `Fallout.Common` in `Directory.Packages.props` move in lockstep, and Dependabot only updates the latter. Fallout's NuGet 11.0.x line is abandoned and unlisted. Never upgrade to it.

## Delegating work to agents

Claude Code subagent definitions are in `.claude/agents/`. The orchestrating session owns design, decisions and integration. An agent gets a written packet and returns a report.

| Agent | Model | Use for |
|---|---|---|
| `spring-grunt` | Sonnet, medium | Mechanical, fully specified work: version bumps from a table, retargets, listed renames or deletions, scripted runs. |
| `spring-implementer` | Sonnet, high | Design-bearing code plus its tests. The packet makes every API, compatibility, dependency and security decision; the agent stops on one the packet leaves open. |
| `spring-reviewer` | Opus, high | Read-only validation of every packet that touches `src/`, and of external PRs before merge. |
| `spring-ci-triage` | Sonnet, medium | Red CI runs and Dependabot PRs. Read-only. |
| `Explore` (built in) | — | Read-only searches where only the conclusion matters. |

A packet states:

- the base sha and the branch (agents that write run in their own worktree);
- the scope and the files the agent must not edit;
- the decisions already made;
- the acceptance criteria;
- the verification commands;
- the commit attribution line.

Agents have this file in context through `CLAUDE.md` and don't Read it again. Keep briefs and reports compact: ask for counts, summaries and failing tails, not file dumps.
