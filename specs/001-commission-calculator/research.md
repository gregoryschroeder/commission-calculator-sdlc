# Research: Quarterly Sales Commission Calculator

Every decision records how it was established (Principle IV): **spiked** (built and run in a
throwaway project outside this repository), **measured**, **vendor-doc/registry** (with the date
consulted), **assumed** (flagged), or **copied** (with its source). Checks were made on
2026-09-21 on macOS arm64 with .NET SDK 10.0.400 unless an entry states another date
(2026-09-22 where noted). No assumed or copied claim is load-bearing for
correctness.

## R1. Runtime and SDK

- **Decision**: .NET 10 (`net10.0`), SDK pinned in `global.json` to 10.0.400 with
  `rollForward: latestFeature`.
- **Established**: vendor metadata — Microsoft `releases-index.json`, 2026-09-21: 10.0 is `active`
  LTS (latest 10.0.12 / SDK 10.0.401); 11.0 is `11.0.0-rc.1`. Measured: `dotnet --version` =
  10.0.400. (Constitution C1.)
- **Alternatives**: .NET 11 RC — not stable, excluded by the brief.

## R2. Web UI technology

- **Decision**: ASP.NET Core Razor Pages, server-rendered, no client-side script. The scenario
  picker is an HTML `<form method="get">` with a labelled `<select>` and a submit button.
- **Rationale**: the UI only displays engine output; a GET form works with keyboard and screen
  reader out of the box, needs no JavaScript and makes every scenario a linkable URL.
- **Established**: spiked — `dotnet new webapp` on SDK 10.0.400 builds and serves `/`; the
  template's static assets are local under `wwwroot/lib` (no CDN). The `lang` attribute on the
  rendered `<html>` was read by a test (AngleSharp) through `WebApplicationFactory`.
- **Alternatives**: Blazor Server (needs a live SignalR circuit and JavaScript for a static
  read-only view — no benefit); minimal API + static HTML (would need client script to render).

## R3. Test framework and runner

- **Decision**: xUnit v3 `xunit.v3` 4.0.1 on **Microsoft.Testing.Platform (MTP)**, opted in via
  `global.json` `"test": { "runner": "Microsoft.Testing.Platform" }`. No `Microsoft.NET.Test.Sdk`,
  no `xunit.runner.visualstudio`.
- **Established**: registry — NuGet, 2026-09-21: `xunit.v3` 4.0.1 published 2026-09-12. Spiked:
  with the VSTest packages, `dotnet test` on SDK 10.0.400 **fails** with "Testing with VSTest target
  is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later"; with the
  `global.json` opt-in the same project runs.
- **Correction recorded**: the first spike used `coverlet.collector` and the VSTest packages from
  memory of the usual template; that combination does not run. Kept visible because the difference
  in method is the lesson (Principle IV).

## R4. BDD tool for Gherkin integration tests

- **Decision**: Reqnroll 3.3.4 with `Reqnroll.xUnit.v3` 3.3.4.
- **Maintenance check** (required by Principle II before adoption): registry and source host,
  2026-09-21 — NuGet: `Reqnroll`/`Reqnroll.xUnit.v3` 3.3.4 published 2026-03-23; GitHub
  `reqnroll/Reqnroll`: not archived, last push 2026-08-26, most recent commit 2026-08-26 ("added
  .NET 10 and TUnit" to the bug template), releases v3.3.2 (2026-01-14), v3.3.3 (2026-01-27),
  v3.3.4 (2026-03-23). Judged currently maintained. SpecFlow, the former .NET default, is not
  considered: Reqnroll is its maintained successor.
- **Compatibility**: `Reqnroll.xUnit.v3` 3.3.4 declares a dependency on
  `xunit.v3.extensibility.core` ≥ 2.0.0, while current xUnit v3 is 4.0.1 — compatibility was
  therefore **spiked**, not assumed: a feature file with a `@FR-006` tag and a step binding ran
  and passed under xunit.v3 4.0.1 + MTP on SDK 10.0.400.
- **Revisit when**: a Reqnroll release declares support for a newer xUnit major, or xUnit v3's
  next major breaks the spiked combination (the CI build would fail loudly).

## R5. No silent skips

- **Decision**: every CI test run passes `--fail-skips on`.
- **Established**: spiked. Without it, a test marked `Skip` produced "Test run summary: Passed!"
  (total 3, succeeded 2, skipped 1) — a false green. With `--fail-skips on`, the same run reported
  "Failed!" (failed 1) and exit code 2. The option is documented by the xUnit v3 runner's own
  `--help` ("treat skipped tests as failures").
- **Positive control**: that skipped probe *is* the control — the gate was seen to fail on a skip
  before being relied on.

## R6. Coverage

- **Decision**: `Microsoft.Testing.Extensions.CodeCoverage` 18.11.2, run with
  `--coverage --coverage-output-format cobertura`; CI reads the cobertura line rate for the engine
  assembly, writes it to the job summary, and fails below 80% (Principle II floor).
- **Established**: registry 2026-09-21 (published 2026-09-11); spiked — produced a
  `*.cobertura.xml` under MTP. `coverlet.collector` is VSTest-only (see R3).

## R7. Test results and traceability input

- **Decision**: `Microsoft.Testing.Extensions.TrxReport` (`--report-trx`). Unit tests carry
  `[Trait("Requirement", "FR-0xx")]`; Gherkin scenarios carry `@FR-0xx` / `@SC-00x` tags.
- **Version**: `Microsoft.Testing.Extensions.TrxReport` 2.4.0 — the version spiked. Registry,
  2026-09-21: 2.4.1 (published 2026-09-16) is the latest; 2.4.0 stays pinned because it is the one
  observed working, and a bump is a normal dependency update.
- **Established**: spiked — in the TRX, a Reqnroll tag appears as
  `<TestCategoryItem TestCategory="FR-006" />` and an xUnit trait value appears in the test's
  properties; both were found by searching the TRX for the IDs.

## R8. Traceability generator and CI helpers

- **Decision**: a console project, `tools/CommissionCalculator.Tools`, with commands `trace`,
  `coverage-gate` and `list-tests`, unit-tested by `tests/CommissionCalculator.Tools.Tests`. `trace`
  reads FR/SC IDs from `spec.md`, test → ID from TRX files, and implementing member → ID from
  `[Implements("FR-0xx")]` attributes (reflection over the built assemblies), and writes
  `specs/001-commission-calculator/traceability.md` with gaps and a scope-only list (FR-019).
- **Correction (2026-09-21, analyze pass 1)**: this first said a single-file program,
  `tools/Traceability.cs`. A single-file program cannot be referenced by a test project, and the
  generator and gates contain real branching logic that the test-first principle requires tests
  for, so it became a project. The spike that `dotnet run file.cs` works on SDK 10.0.400 still
  stands; it is no longer relied on.

## R9. Integration test host for the UI

- **Decision**: `Microsoft.AspNetCore.Mvc.Testing` 10.0.12 (`WebApplicationFactory<Program>`) and
  AngleSharp 1.8.2 to parse and assert rendered HTML (FR citations, table headers, labels).
- **Established**: registry 2026-09-21; spiked — a test fetched `/` and asserted `lang="en"`.

## R10. Exact money arithmetic

- **Decision**: all money is `decimal`; rounding is
  `Math.Round(x, 2, MidpointRounding.AwayFromZero)` (FR-018); whole-cent validation is
  `decimal.Round(x, 2) == x`; percentages are `decimal`.
- **Established**: spiked — `Math.Round(0.505m, 2, AwayFromZero)` = 0.51 and of −0.505m = −0.51;
  `JsonSerializer.Deserialize<decimal>("10.005")` = 10.005 exactly (scale 3), so a sub-cent input
  is detectable after deserialization (FR-004).

## R11. Dates

- **Decision**: `DateOnly`; inclusive day counts as `end.DayNumber − start.DayNumber + 1`.
- **Established**: spiked — 2026-05-16..2026-06-30 = 46 (spec US3 AS3).

## R12. How the brief's "`dotnet run`" is met

- **Finding**: spiked — with a solution file at the repository root, `dotnet run` there fails:
  "Couldn't find a project to run … or pass the path to the project using --project."
- **Decision**: the web project is the only runnable project; run it with
  `dotnet run --project src/CommissionCalculator.Web` from the root, or plain `dotnet run` from
  `src/CommissionCalculator.Web`. Both are documented in the README; CI exercises the first (the
  smoke step starts the app with `--project` and requests `/`). Putting the web `.csproj` at the root was rejected: its
  default file globs would compile the engine, tests and tools into the web assembly.
- **Maintainer approval**: this interpretation of constraint C2 was approved at the plan gate,
  2026-09-21 ("2: yes", plan.md decision 2).

## R13. Seed data format

- **Decision**: one JSON file per seeded scenario under `src/CommissionCalculator.Web/Scenarios/`,
  copied to the output directory and read at startup (the brief allows "a local file"). The files
  are the seeded scenarios, and their full expected statements are spec.md Appendix A
  (maintainer decision 1A), so SC-001's expected values are the spec's. (Earlier wording, before
  Appendix A: "each seeded rep corresponds to an acceptance example".)
- **Established**: R10 spike (exact decimal parsing); `System.Text.Json` is part of the shared
  framework (no package).

## R14. CI

- **Decision**: GitHub Actions on `ubuntu-latest`, `actions/checkout@v7`,
  `actions/setup-dotnet@v6` (reads `global.json`), `actions/upload-artifact@v7`,
  `actions/download-artifact@v8`; offline check uses `curlimages/curl:8.22.0`.
- **Established**: source host, 2026-09-21 — latest releases: checkout v7.0.1 (2026-07-20),
  setup-dotnet v6.0.0 (2026-07-16, documents installing the `global.json` SDK when no version is
  given), upload-artifact v7.0.1 (2026-04-10). **Assumed until the first CI run**: that the
  runner resolves SDK 10.0.4xx via setup-dotnet (constitution C7 records the same assumption).
  Consulted 2026-09-22: download-artifact latest v8.0.1 (2026-03-11); Docker Hub `curlimages/curl`
  latest tag 8.22.0 (2026-09-02). **Assumed until the traceability job first runs**: that
  download-artifact v8 reads artifacts written by upload-artifact v7 (not checked).
- **Not in CI**: the kit's `install.sh --check` — the library is private and the public
  repository's CI cannot fetch it. Run locally after any SpecKit upgrade (docs/PROVISIONING.md).

## R15. Per-test isolation

- **Decision**: the engine is pure (no I/O, no clock, no statics with state); seed files are
  read-only. CI additionally runs every test in every test project individually
  (`--filter-method` per fully qualified `className.name` read from the suite run's TRX), so a test
  depending on another would fail there.
- **Order (spiked 2026-09-21, found by analyze)**: xUnit v3 4.0.1's runner accepts a `[:seed]`
  argument, but seeds 1, 2 and 3 all ran one class's five tests in the same order (T3, T5, T1, T2,
  T4 — not declaration order), so a different order cannot be forced cheaply. "Any order" is
  therefore enforced by design (no mutable shared state, Principle II) and by the per-test run, not
  by reordering. Residual risk, stated rather than hidden: a test that leaves behind state that a
  later test reads would only be caught if that state were shared, which the design forbids.
- **Established**: spiked — `--filter-method` selected tests under MTP; `--list-tests` lists
  Reqnroll scenarios by display name ("rounding") rather than method name, which is why the loop
  takes names from the TRX `<TestMethod className=… name=…>` (e.g. `Bdd.Features.AddFeature` /
  `Rounding`) instead. **Assumed**: the per-test loop adds under two minutes to CI — to be
  measured on the first run.

## R16. Offline smoke run (SC-004)

- **Decision**: a CI job runs the published web app in `mcr.microsoft.com/dotnet/aspnet:10.0.12`
  with `docker run --network none` and requests a scenario page from a sidecar container that
  shares its network namespace (see the request-method spike below), so the
  "no network service at run time" claim is shown under the condition, not reasoned.
- **Established**: registry — `mcr.microsoft.com/v2/dotnet/aspnet/tags/list`, 2026-09-21, lists
  `10.0.12`, `10.0` and `10.0-noble`. **Assumed until the first run of that job**: Docker is
  available on `ubuntu-latest` runners.
- **Request method — spiked locally (Docker 29.8.0, 2026-09-21)**: the `aspnet:10.0.12` image has no
  `curl` or `wget`, so the request cannot come from inside the app container. A published
  `dotnet new webapp` ran with `--network none` (`docker inspect` → `NetworkMode=none`); a
  `curlimages/curl` container started with `--network container:<app>` got HTTP 200 from
  `http://127.0.0.1:8080/`, and its request to `https://example.com/` failed (curl exit 28) — the
  negative control showing the shared namespace really has no network. (Found by analyze pass 4.)

## R17. Seed files reach build and publish output by default

- **Decision**: no project-file entry for `Scenarios/*.json`.
- **Established**: spiked on SDK 10.0.400 (2026-09-21, found by analyze pass 2 and re-run
  independently): in a `dotnet new webapp` project, `Scenarios/x.json` appeared in
  `bin/Debug/net10.0/Scenarios/` and in `dotnet publish` output with no project-file entry; adding
  `<Content Include="Scenarios/*.json" …>` failed the build with NETSDK1022 (duplicate Content
  items); `<Content Update="Scenarios/*.json" CopyToOutputDirectory="Never" …>` removed the file
  from build output — the positive control that makes the smoke guard (tasks T060b) able to fail.
