# AGENTS.md

Guidance for AI coding agents working in this repository. Human-facing documentation is in README.md.

## Project overview

Spring.NET is a port and extension of the Java Spring Framework for .NET: IoC container, AOP, expression language, declarative transaction management, ADO.NET framework, ASP.NET (WebForms/MVC5/WebAPI) integration, NHibernate 5, Quartz.NET, messaging (MSMQ, NMS), and NUnit/MSTest testing support. Apache 2.0 licensed. Packages are published to NuGet from git tags (`v*.*.*`) via NuGet Trusted Publishing (GitHub OIDC), not a stored API key.

## Build and test commands

Build orchestration is Fallout (`build-support/build`), the maintained successor to NUKE. Root-level bootstrap scripts install the pinned .NET SDK if missing (`global.json`: 10.0.100, rollForward latestMinor), then run the build through the `fallout` local tool (`dotnet tool restore` + `dotnet fallout`).

```
./build.cmd            # Windows (build.sh on Linux/macOS); default target = Compile
./build.cmd Compile    # build Spring.Net.sln (+ examples, Windows only)
./build.cmd Test       # run the default test suite
./build.cmd Ci         # Compile + Test + Pack — what CI runs
./build.cmd Clean
```

Plain `dotnet` CLI works too — the solution is `Spring.Net.sln`:

```
dotnet build Spring.Net.sln
dotnet test --project test/Spring/Spring.Core.Tests/Spring.Core.Tests.csproj
dotnet test --project test/Spring/Spring.Core.Tests/Spring.Core.Tests.csproj -f net10.0 --filter "FullyQualifiedName~XmlObjectFactoryTests"
```

- Test framework is **NUnit 5** (`Spring.Testing.Microsoft.Tests` alone uses MSTest 4), asserting through the constraint model (`Assert.That`, never `ClassicAssert`); mocking uses **FakeItEasy**. Tests run on **Microsoft.Testing.Platform**, selected in `global.json`: every test project is an executable (`OutputType` Exe, `EnableNUnitRunner`), `dotnet test` takes the project as `--project <path>` rather than positionally, and there is no `Microsoft.NET.Test.Sdk`. `./build.cmd Test` writes a TRX file per project under `artifacts/TestResults/`. The classic assert model (`ClassicAssert`, `StringAssert`, `CollectionAssert`, `FileAssert`, `DirectoryAssert`, `Assert.AreEqual` and the rest) is a build error: `test/Directory.Build.targets` drops `nunit.framework.legacy.dll` from the compiler's references. NUnit1032 (dispose an `IDisposable` field in `[TearDown]`, or `[OneTimeTearDown]` for one set in `[OneTimeSetUp]`) is enforced at its default severity.
- CI (`.github/workflows/ci.yml`) runs `./build.cmd ci` on windows-latest and `./build.sh ci` on ubuntu-latest for every PR to `main`.
- **Warnings are errors** in `src/` and `test/` (`TreatWarningsAsErrors=True`), and formatting rule IDE0055 is warning severity — a formatting violation fails the build.
- Build output goes to `build/$(Configuration)/$(ProjectName)/` (not in-place `bin/`); NuGet packages to `artifacts/`.

### Integration tests

Fallout's `Test` target excludes `*.Integration.Tests` projects (and `Spring.Web.Conversation.NHibernate5.Tests`) by default; they need external infrastructure and are gated behind flags: `--test-full`, `--test-integration-data`, `--test-integration-nms`, `--test-integration-ms-mq`. Several test projects are Windows-only (net472: Web, Mvc5, Messaging/MSMQ).

- SQL Server-backed tests (`Spring.Data.Integration.Tests`, `Spring.Data.NHibernate5.Integration.Tests`, `Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests`, `Spring.Scheduling.Quartz3.Integration.Tests`) provision their own database with Testcontainers: a running Docker daemon is all they need. `./build.cmd IntegrationTest` runs them (CI: the `integration-linux` job); `--test-integration-data` adds them to `Test`. Each assembly's `[SetUpFixture]` calls `test/Shared/SqlServerTestEnvironment.cs` (a linked source file), which starts a pinned SQL Server container, runs `build-support/create-integration-test-databases-and-users.sql` and the project's `*_database.sql` scripts, and publishes `SPRING_TEST_SQLSERVER_*` environment variables that the XML contexts read as `${...}` placeholders. Setting `SPRING_TEST_SQLSERVER` to an admin connection string uses that server instead of a container. Tests that need MSDTC are `[Explicit]`. See README.md "Running tests".
- ActiveMQ-backed tests (`Spring.Messaging.Nms.Integration.Tests`, net10.0 and net472) provision a pinned ActiveMQ Classic container the same way, through `test/Shared/ActiveMqTestEnvironment.cs`, which publishes `SPRING_TEST_ACTIVEMQ_BROKER_URI` (`tcp://host:port`) for the XML contexts. Setting that variable beforehand uses that broker instead of a container. `./build.cmd IntegrationTest` runs this project too; `--test-integration-nms` adds it to `Test`.

## Architecture

`src/Spring/` has one directory per shipped assembly. All projects use `RootNamespace=Spring` and source folders mirror namespaces. Dependency spine: **Spring.Core → Spring.Aop → Spring.Data → everything else**.

| Project | Purpose |
|---|---|
| Spring.Core | IoC container, expression language, validation, dynamic reflection |
| Spring.Aop | AOP framework (dynamic proxies, advice, pointcuts) |
| Spring.Data | Transaction management + ADO.NET framework |
| Spring.Data.NHibernate5 | NHibernate 5 integration (RootNamespace `Spring.Data.NHibernate`) |
| Spring.Web / .Mvc5 | ASP.NET WebForms / MVC5+WebAPI integration (net472 only) |
| Spring.Messaging / .Nms | MSMQ / Apache NMS integration |
| Spring.Scheduling.Quartz3 | Quartz.NET 3.x integration |
| Spring.Testing.NUnit / .Microsoft | DI-enabled test fixture base classes |

Key mechanics (defined in `src/Directory.Build.props` and `test/Directory.Build.props` — there is **no root** Directory.Build.props):

- **Multi-targeting**: most libraries target `netstandard2.0;net472;net10.0`; web projects are net472-only. Projects reference the shared MSBuild variables `$(TargetFullFrameworkVersion)` (= net472) and `$(TargetModernFrameworkVersion)` (= net10.0) instead of literal TFMs. Framework differences are handled with conditional `<Compile Remove>`/`<ItemGroup>` blocks in csproj files, not scattered `#if` directives. Key them to what they mean (`'$(TargetFrameworkIdentifier)' == '.NETFramework'` or `NETFRAMEWORK`, never "not netstandard"). The one exception is the legacy-serialization obsoletion: `ObsoleteAttribute.DiagnosticId` only exists on modern .NET, so each serialization constructor and `GetObjectData` carries a `#if NET` guard around its `[Obsolete(Obsoletions...)]` (constants in `src/Spring/Obsoletions.cs`, linked into each library).
- **Strong naming**: all assemblies signed with the committed `Spring.Net.snk`.
- **Central package management**: versions live in `Directory.Packages.props`; csproj `PackageReference`s are versionless.
- **Expression parser**: the grammar is `src/Spring/Spring.Core/Expressions/ExpressionGrammar.parlot.cs`, a build-only file that the `Parlot.SourceGenerator` analyzer turns into code of `Spring.Core`, with no runtime dependency on Parlot. It needs the Roslyn of the .NET 10 SDK. Its tokens, tree and syntax errors are in `ExpressionGrammar.cs`, and `ExpressionGrammarTests` holds the trees of the former ANTLR parser. `src/Spring/Spring.Core/Expressions/README.md` has the quirks kept on purpose and how the grammar stays equivalent.
- **Versioning**: the git tag is the version authority (`v3.1.0` → 3.1.0); untagged builds fall back to the `ProjectVersion` placeholder in `build-support/build/Build.cs` plus a `dev-`/`preview-` suffix. Publishing runs from the tag workflow only. `DetermineVersion()` sets three distinct facets, applied to **both** `Compile` and `Pack` so packaged and `bin/` assemblies agree: `AssemblyVersion` is pinned to `major.minor.0.0` (the assemblies are strong-named, so this is their binding identity — keeping it stable across patches means consumers need no new binding redirect for a fix release), `FileVersion` carries the exact numeric version, and `InformationalVersion` is left to MSBuild so SourceLink can append `+<sha>`. Never set a bare `Version` property instead — it overrides the prefix/suffix composition and silently drops the preview suffix.
- **Trusted publishing**: `.github/workflows/publish.yml` is the only workflow allowed to push to nuget.org. It needs `permissions: id-token: write` and `environment: nuget`; the `Publish` target exchanges that OIDC token for a short-lived API key in `build-support/build/Build.Publish.cs`. The nuget.org policy is keyed to the **workflow filename** `publish.yml` plus the `nuget` environment, so renaming, moving or splitting that workflow breaks publishing until the policy is updated. Do not reintroduce a `NUGET_API_KEY` secret — the `NuGetApiKey` parameter stays only as a manual override.
- **Package READMEs and package validation**: every packable project under `src/Spring` has a `README.md` next to its csproj, which `src/Directory.Build.props` packs as the NuGet readme; a new package needs one (what it does, `dotnet add package`, target frameworks, a minimal snippet whose types, members and XML are checked against the code, and links). Pack also runs package validation (`EnablePackageValidation`) against the 3.1.0 packages (`PackageValidationBaselineVersion`) and between the target frameworks of one package; never suppress a difference between two TFMs of one package (Spring.Messaging turns validation off for that reason, see its csproj). Accepted baseline differences live in each project's `CompatibilitySuppressions.xml`; regenerate one with `dotnet pack <csproj> -c Release -p:ApiCompatGenerateSuppressionFile=true -p:AssemblyVersion=<major>.<minor>.0.0` and review the diff. A direct `dotnet pack` needs that `-p:AssemblyVersion=<major>.<minor>.0.0` (what Fallout passes), because the default 1.0.0.0 is below the baseline and fails with CP0003; do not gate the baseline on AssemblyVersion, since Fallout's Restore passes no version and the baseline download would vanish. The baseline has a coverage gap: 3.1.0 shipped net462, which no 4.0 asset is compatible with, so Spring.Web, Spring.Web.Mvc5, Spring.Web.Conversation.NHibernate5 and Spring.Testing.NUnit get no baseline API comparison (PKV006), and for the other packages the 3.1.0 net462 asset is compared with the 4.0 netstandard2.0 build, so no net472 build is compared with 3.1.0. The gap closes when the baseline moves to 4.0.0 once 4.0.0 ships; then delete the 3.1.0 suppression files.
- In test projects, `ILog` is a global using alias for `Microsoft.Extensions.Logging.ILogger`.

## Code style

`.editorconfig` is authoritative: 4-space indents (2 for XML/props/ps1), Allman braces, `System` usings first, no `this.` qualification, language keywords over BCL type names, instance fields `_camelCase`, static fields `s_camelCase`. Match the surrounding code's style. (The `[src/{Analyzers,...}/**]` sections were copied from the Roslyn repo and match nothing here — ignore them.)

**Logging**: all `ILogger` calls use constant message templates with PascalCase named placeholders (`log.LogDebug("Creating object '{ObjectName}'", name)`) — never interpolation, concatenation, or `string.Format`. Pass exceptions to the exception-first overloads; wrap inherently dynamic text as `Log*("{Message}", text)`. Analyzers CA2254/CA2253/CA2017/CA1727 enforce this as build errors. Only guard a log call with `IsEnabled` when argument evaluation is expensive (string building, method calls) — templates already defer formatting.

## Legacy areas — do not modify

- `doc/` — DocBook reference sources and their vendored toolchain (CI ignores `doc/**`); kept until the documentation moves to Markdown.

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
