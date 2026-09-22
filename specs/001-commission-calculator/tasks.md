---
description: "Task list for the quarterly sales commission calculator"
---

# Tasks: Quarterly Sales Commission Calculator

**Input**: `specs/001-commission-calculator/` — plan.md, spec.md (incl. Appendix A), research.md,
data-model.md, contracts/, quickstart.md

**Tests**: mandatory in every phase (constitution Principle II; standing rules below). Every
test-writing task precedes the implementation task it guards, and its tests are run and **shown
failing** before that implementation starts.

**Format**: `[ID] [P?] [Story] Description` — `[P]` = parallelizable (different files, no
dependency on an incomplete task).

## Standing rules for this task list

Restated for this project from the installed tasks-template rules and the constitution, so the
analyze step can hold this list to them. Where anything above or below conflicts, these win.

1. **Tests are never optional.** Any task that adds real branching logic — in any phase, including
   Setup and Foundational — is preceded by a task that writes its tests, and those tests are seen
   failing first. A task that is purely structural (records, a DTO, wiring with no branch) says so
   and has no dedicated test. A CI gate step (a shell step whose only job is to fail the build) is
   tested by being **seen failing** on a deliberate probe, recorded in the task.
2. **Failure-path guards are watched failing.** A test whose job is to catch something going wrong
   (a rejection, a clawback, a negative-earnings path, a CI gate) names how its guarded behaviour
   will be removed to show the test failing, and the observed result is written into the task when
   it is ticked. If a guard cannot be made to fail, that is recorded as a finding. Polish re-checks
   every such guard (T063).
3. **Condition-dependent checks evidence the condition.** A check that only means something under
   a condition (fresh clone, no network, keyboard only, screen reader on) records evidence that the
   condition held just before the observation.
4. **Every phase is one pull request.** Each phase ends with a checkpoint task: open the phase's
   PR, CI green, maintainer approves the squash-merge. No PR spans two phases; phases run in order.
5. **Runtime dependencies are tasks.** This feature adds one runtime dependency: the seeded
   scenario JSON files, added in Phase 9 (T060a) after the check that compares them with Appendix A
   (T060) is written and seen failing. The Web SDK's default content items copy them to build and
   publish output (T025, research R17), and the smoke assertion added in T060b proves the running
   app loads them. Before Phase 9, host-level tests use scenario folders they create themselves. It adds one
   optional configuration value, `Scenarios:Directory` (default: `Scenarios` under the app's output
   directory), bound through standard ASP.NET Core configuration and documented in the README
   (T011, T024); no environment variables are required, and no secrets or services.
**Correction at the Phase 6 gate (2026-09-22, maintainer approved).** A Phase 5 test asserted
`RequirementId == "FR-004"` for every validation rule, including the start-date rules that FR-011
owns and Appendix A.11 cites as FR-011. The spec was right and the test was wrong, so the test now
carries the expected requirement per rule (FR-011 for the start-date rules, FR-013 for the split
sum, FR-004 for the rest) and the validator cites them. No payout changed.

6. **Every test names what it verifies**: `[Trait("Requirement", "FR-0xx")]` on unit tests,
   `@FR-0xx` / `@SC-00x` tags on Gherkin scenarios; tests of project tooling carry
   `[Trait("Principle", "II")]` / `("III")` for the principle they enforce (constitution v1.1.0).
   Tests that exercise the running web host are integration tests and are Gherkin scenarios in the
   Specs project; only single-class checks (file reader, catalog, stylesheet contrast) are xUnit. Every acceptance example is tested with its
   exact inputs and asserted to the cent — breakdown lines *and* statement summary values
   (attainment, credited bookings, total draw, re-split shares). A test is never edited to make it
   pass; a test that looks wrong is a spec question for the maintainer.

7. **Red or green at write time is declared, never assumed.** "Run; record failing" means the
   whole task's tests are expected red. Where a test already passes when written, because an
   earlier phase built what it checks, the task says so by name and names a guard that can make
   it fail; the recorded result says which tests were red and which green. So that a declared-green
   test can run, and every red fails on an assertion rather than a compile error, a test-writing
   task first adds compile-only stubs for any new type or member its tests reference (signatures
   only, bodies `throw new NotImplementedException()`); the implementation task replaces them. This
   applies in particular to T030 (formatters), T047 (`SplitAllocation`) and T056
   (`ClawbackCalculator`), whose projects also hold that task's declared-green tests.
8. **Validation scope.** Each validation rule is tested once, in the task that introduces it, in
   every place it applies (scenario roster and deal list, booking-quarter reps, partners and deals,
   refunds), with a guard that restricts it to the scenario's own data and sees the other cases
   fail. Later tasks do not re-test it. Test data for any validation or acceptance case is fully
   valid under **every** FR-004/FR-011/FR-013 rule, including rules that later phases add, except
   for the one violation under test — booking-quarter cases start from Appendix A.10's booking-
   quarter block — so no rule landing later can reject an earlier test's data (T018, T038, T043,
   T047, T056).
---

## Phase 1: Setup (shared infrastructure) — PR 1

**Purpose**: solution skeleton, build rules, CI helper tools and the CI review gate.

- [X] T001 Create `global.json` (SDK 10.0.400, `rollForward: latestFeature`, test runner
  `Microsoft.Testing.Platform`), `Directory.Build.props` (net10.0, nullable, implicit usings,
  `TreatWarningsAsErrors`, `AnalysisLevel` latest-recommended, `EnforceCodeStyleInBuild`; and,
  for every project under `tests/`, package references to `Microsoft.Testing.Extensions.TrxReport`
  and `Microsoft.Testing.Extensions.CodeCoverage`, because CI passes `--report-trx --coverage` to
  every test project and a project without them exits 5, "unknown option"),
  `Directory.Packages.props` (central versions per plan.md), `.gitignore`, and
  `CommissionCalculator.slnx` — structural, no test.
  **Result**: Done 2026-09-22.
- [X] T002 Create `src/CommissionCalculator.Engine/CommissionCalculator.Engine.csproj` (no package
  references; `<InternalsVisibleTo Include="CommissionCalculator.Engine.Tests" />` so unit tests can
  reach internal calculation types — otherwise Engine.Tests fails with CS0122) and `src/CommissionCalculator.Web/CommissionCalculator.Web.csproj` (Razor Pages,
  references Engine only) with a minimal `Program.cs` — an empty pipeline plus `public partial class
  Program` — so the project builds and T003 fails on its assertions (`/` returns 404), not on a
  missing entry point (CS5001) — structural, no test.
  **Result**: Done 2026-09-22; solution builds clean with warnings as errors.
- [X] T003 Create `tests/CommissionCalculator.Specs/` (xunit.v3 — Reqnroll.xUnit.v3 does not bring
  the runner, and without it the project fails with CS5001 — plus Reqnroll.xUnit.v3, Mvc.Testing,
  AngleSharp; TrxReport and CodeCoverage come from T001) with a `WebApplicationFactory<Program>` hook, and write the first
  scenario **before** the page exists: `Features/AppHost.feature` (`@FR-020`) — "the app serves its
  page": GET `/` returns 200 with `<html lang="en">` and one `<main>`. Run it; record it failing.
  **Result**: Recorded red 2026-09-22: `Assert.Equal() Failure — Expected: OK, Actual: NotFound` (empty pipeline, 404).
- [X] T004 Implement the minimal `Program.cs` and `Pages/Index.cshtml` (layout with `lang`, skip
  link, one `<h1>`, `<main>`, local `wwwroot/css/site.css`) until T003 passes. (`Web.Tests` is created in T022
  together with its first tests: a test project with no tests makes `dotnet test` exit non-zero.)
  **Result**: Done; T003 green.
- [X] T005 Create `tests/CommissionCalculator.Tools.Tests/` and `tools/CommissionCalculator.Tools/`
  with compile-only stubs of the `coverage-gate` and `list-tests` commands (standing rule 7), and
  write tests for those commands **before** their logic exists (`[Trait("Principle", "II")]`) (fixture files under `tests/CommissionCalculator.Tools.Tests/
  Fixtures/`): `CoverageGateTests` — engine line rate 0.85 passes, 0.79 fails, report with no
  engine package fails *unless* the engine assembly contains no types (then passes with the message
  "no engine lines yet"), using a fixture report that *has* an engine package so the floor can
  bite; several reports (one per test project) are merged — the engine's lines are the union
  across reports and a line counts as covered if any report covers it — with fixtures where one
  report lacks the engine package and another covers it partly; `TrxTestListTests` — lists fully qualified `className.name` for xUnit and
  Reqnroll tests from a fixture TRX (the Reqnroll display-name case from research R15). Run; record
  failing.
  **Result**: Recorded red 2026-09-22: all 6 tests failed with `NotImplementedException` from the stubs (no compile errors). Test names are PascalCase because the analyzers reject underscores (CA1707).
- [X] T006 Implement `tools/CommissionCalculator.Tools/` (console app with
  `<FrameworkReference Include="Microsoft.AspNetCore.App" />`, so `trace` can reflect over the web
  assembly — research R8; commands `coverage-gate` and `list-tests`) until T005 passes. (Replaces research R8's single-file program: a project can be
  unit-tested; see research R8 correction.)
  **Result**: Done; 6/6 green. `Program.cs` is a thin argument switch with no unit test of its own; it is exercised end to end by the CI steps and the T007/T008 guards.
- [X] T007 Write `.github/workflows/ci.yml`: checkout@v7, setup-dotnet@v6 from `global.json`,
  `dotnet build -warnaserror`, `dotnet test --fail-skips on --report-trx --coverage
  --coverage-output-format cobertura`, `coverage-gate` over **all** cobertura files from the
  run, merged per T005 (floor 80% on the engine, rate written to the job summary), upload TRX and coverage as artifacts.
  *Guard (warnings)*: on a local throwaway change add an unused variable in Engine; confirm the
  build fails; revert; record. *Guard (coverage)*: run `coverage-gate` with floor 101% against the
  Tools.Tests fixture report that contains an engine package (the real report has no engine lines
  until Phase 2); confirm non-zero exit; record. Repeat against the real report in T063.
  **Result**: Guards recorded 2026-09-22: unused variable in Engine → `error CS0219`, build exit 1; `coverage-gate 101` on the fixture with an engine package → "85.00% (floor 101%) — FAIL", exit 1. Real run: "no engine lines yet", exit 0.
- [X] T008 CI per-test isolation step: for **every** test in all test projects (list from the suite
  TRX via `list-tests`), run `dotnet test --project <proj> --filter-method <className.name>` alone;
  fail if any fails. *Guard*: on a local throwaway branch add two Tools tests where one passes only
  when the other ran first (static flag); confirm the suite passes and this step fails; delete it;
  record. Record the step's CI duration in research R15 on the first run.
  **Result**: Guard recorded 2026-09-22: with an order-dependent probe pair the suite passed ("Test run summary: Passed!") and this step failed the reader test run alone, exit 1. First CI run: 12 s for 7 tests (research R15).
- [X] T009 *Guard (skip gate)*: on a local throwaway change add `[Fact(Skip="probe")]`, run the CI
  test command; confirm a non-zero exit ("Failed!"); remove it; record.
  **Result**: Guard recorded 2026-09-22: `[Fact(Skip="probe")]` → "Test run summary: Failed!", failed 1, exit code 2.
- [X] T010 CI **smoke job** (its own job, so nothing has been built before it): check out, set up
  the SDK, print `git status --ignored` (evidence of a clean tree: no `bin/`/`obj/`) immediately
  before the run, then start `dotnet run --project src/CommissionCalculator.Web` in the background, poll `/` until HTTP 200 (fail after 60 s), stop
  it. *Guard*: point the poll at a path that returns 404 and confirm the step fails; record.
  **Result**: Guard recorded 2026-09-22: `smoke.sh /` → 200, exit 0; `smoke.sh /does-not-exist` → "did not return 200 within 60 s (last status: 404)", exit 1.
- [X] T011 `README.md`: what it is, `dotnet run --project src/CommissionCalculator.Web` (or
  `dotnet run` inside that folder), test commands, where the spec lives, and the optional
  `Scenarios:Directory` setting (e.g. `--Scenarios:Directory=/path` on the command line).
  **Result**: Done; plain `dotnet run` inside `src/CommissionCalculator.Web` verified to serve HTTP 200. The `Scenarios:Directory` setting is marked as arriving in Phase 2.
- [X] T012 Checkpoint: open PR "Phase 1: Setup", CI green, maintainer approves squash-merge. Record
  the first CI run's outcome in research R14 (SDK resolution) and R15 (isolation-step time), and
  propose to the maintainer the PATCH amendment that marks constitution C7 as confirmed and, in its
  pairwise note, qualifies "CI installs the pinned SDK" as confirmed by this run (until then it is
  assumed, as research R14 says), and records in C2 and Principle VII the approved reading of "runs
  with `dotnet run`" (`dotnet run --project src/CommissionCalculator.Web`, plan decision 2).

  **Result**: PR #2 CI green (runs 35758821698, 35759089614); squash-merged 2026-09-22 with the
  maintainer's approval. R14/R15 recorded; constitution PATCH v1.1.1 approved and made in its own PR.
---

## Phase 2: Foundational (blocking prerequisites) — PR 2

**Purpose**: engine contract types, money rules, the engine entry point and the scenario-file
reader that every story uses.

- [X] T013 Create the engine's public input/result records and `ImplementsAttribute` exactly as in
  contracts/engine-api.md (`Inputs/`, `Results/`) — structural, no test.
  **Result**: Done 2026-09-22; contract types as in contracts/engine-api.md.
- [X] T014 Create `tests/CommissionCalculator.Engine.Tests/` and write `MoneyTests`
  (`FR-018`, `FR-004`): half away from zero (0.505→0.51, −0.505→−0.51, 1548.387…→1548.39,
  50549.4505…→50549.45); whole-cent check true for 10.10, false for 10.005. Run; record failing.
  **Result**: Recorded red 2026-09-22: 6/6 failed with `NotImplementedException` (stubs).
- [X] T015 Implement `Calculation/Money.cs` until T014 passes.
  **Result**: Done; MoneyTests green.
- [X] T016 Write `QuarterTests` (`FR-004`, `FR-014`): 2026-01-01..03-31 has 90 days and months
  Jan/Feb/Mar; 2026-04-01..06-30 has 91; a quarter not starting on the 1st or not spanning three
  whole months is invalid. Run; record failing.
  **Result**: Recorded red: 7/7 failed with `NotImplementedException`.
- [X] T017 Implement quarter day/month helpers (internal, in `Calculation/`) until T016 passes.
  **Result**: Done; QuarterTests green.
- [X] T018 Write `ScenarioValidatorTests` for the shared rules (`FR-004`), each tested in **every
  place it applies** — the scenario's roster and deal list, booking-quarter reps, partners and
  deals, and refunds (validation scope rule, below): quota ≤ 0; deal amount ≤ 0; refund amount ≤ 0;
  any monetary input (quota, deal amount, refund amount, opening balance) not a whole number of
  cents; negative opening balance; empty roster; bad quarter shape (scenario and booking quarter);
  a scenario deal credited to a rep not on the roster; same rep twice on a deal; two roster reps
  with one repId; two deals in one list with one dealId; split % ≤ 0 or > 100. Each test asserts
  that its own error is among those listed and the scenario is rejected (not an exact error set:
  later phases add rules such as FR-013 that a > 100% split also trips, and a test is never edited
  to pass); one further test with three mutually independent violations (quota ≤ 0, sub-cent deal
  amount, negative opening balance) asserts that all three are listed. *Guard*: for each rule,
  comment out that rule's check and confirm its tests fail; separately, validate only the
  scenario's own roster and deal list and confirm every booking-quarter case fails; record. Run;
  record failing.
  **Result**: Recorded red: 30/30 failed with `NotImplementedException`. The theory's data holds delegates, and each case still reports by name. Guards recorded 2026-09-22: disabling each of the 16 checks failed exactly its own cases (e.g. split range → 4 cases, duplicate dealId → 2, quota > 0 → 3 plus the multi-error test); restricting validation to the scenario's own data failed all 11 booking-quarter cases. A guard written as `if (false)` does not compile under warnings-as-errors (CS0162), so checks were disabled with a non-constant false condition.
- [X] T019 Implement `Validation/ScenarioValidator.cs` (shared rules) until T018 passes.
  **Result**: Done; ScenarioValidatorTests green.
- [X] T020 Write `CommissionEngineTests` (`FR-003`, `FR-004`, `FR-005`): an invalid scenario returns
  `RejectedScenario` and no statements; a valid one returns one `RepStatement` per roster rep in
  roster order whose first line is "Quarterly quota" citing FR-005; every line's RequirementId
  matches `FR-\d{3}`. *Guard*: skip the validation call and confirm the rejected-scenario test
  fails; record. Run; record failing.
  **Result**: Recorded red: 4/4 failed with `NotImplementedException`. Guard: skipping validation failed `AnInvalidScenarioIsRejectedWithNoStatements`.
- [X] T021 Implement `CommissionEngine.Calculate` and `Calculation/StatementBuilder.cs` (validation
  first, then statement assembly) until T020 passes.
  **Result**: Done; engine 47/47 green.
- [X] T022 Create `tests/CommissionCalculator.Web.Tests/` (xunit.v3 unit tests of single web
  classes) and write `ScenarioFileReaderTests` (`FR-001`, `FR-004`): a file matching
  contracts/scenario-file.md maps to the expected `ScenarioInput`; `10.005` is read exactly;
  unknown property, malformed JSON and a missing required field each produce a load error naming
  the file, never an exception out of the reader. *Guard*: remove the reader's error capture and
  confirm the malformed-JSON test fails with an escaped exception; allow unmapped JSON members and
  confirm the unknown-property test fails; drop the required-field check and confirm the
  missing-field test fails; record all three. Run; record failing.
  **Result**: Recorded red: 5/5 failed with `NotImplementedException`. Guards: catching the wrong exception type failed all three unreadable-file cases (and the catalog's does-not-hide test); allowing unmapped members failed the unknown-property case; dropping the required-parameter check failed the missing-field case.
- [X] T023 Write `ScenarioCatalogTests` in Web.Tests (`FR-001`): files are listed ordered by file
  name; a file that fails to load is listed as a load error and does not hide the others; two
  files declaring the same scenario id are both reported as a load error naming the id (FR-004); a
  scenario directory that does not exist yields an empty catalog, not an exception (the shipped app
  has no `Scenarios/` folder until Phase 9). *Guard*: remove the existence check and confirm that
  test fails with the escaped exception; skip the duplicate-id check and confirm the duplicate-id
  test fails; record.
  *Guard*: make the catalog stop at the first failing file and confirm the "does not hide the
  others" test fails; record. Run; record failing.
  **Result**: Recorded red: 4/4 failed with `NotImplementedException`. Guards: removing the existence check failed the missing-directory test; skipping the duplicate-id check failed the duplicate test; stopping at the first failing file failed the does-not-hide test.
- [X] T024 Implement `Catalog/ScenarioFileReader.cs` and `Catalog/ScenarioCatalog.cs` (folder
  `Catalog`, not `ScenarioCatalog`, so the folder-derived namespace does not collide with the class
  name — CS0118) (loads every
  `Scenarios/*.json` from a directory set by an options value that defaults to
  `Path.Combine(AppContext.BaseDirectory, "Scenarios")` — the build/publish output, not the content
  root, so a file missing from output is missing at run time; the value `Scenarios:Directory` can
  override it, and tests point it at a directory they create) until T022–T023 pass.
  **Result**: Done; Web.Tests 9/9 green. `Scenarios:Directory` is bound through `IOptions<ScenarioOptions>`; the host-level test of that binding comes with T030.
- [X] T025 Confirm in the real solution that `Scenarios/*.json` reaches build and publish output
  through the Web SDK's default content items, with **no** project-file entry (research R17: an
  explicit `<Content Include>` fails the build with NETSDK1022) — structural; proven by T060b's smoke
  assertion and guard.
  **Result**: Confirmed 2026-09-22: a probe `Scenarios/probe.json` reached `bin/Debug/net10.0/Scenarios/` with no project-file entry (the csproj mentions `Scenarios` nowhere).
- [ ] T026 Checkpoint: PR "Phase 2: Foundational", CI green, maintainer approves squash-merge.

---

## Phase 3: User Story 1 — tiered commission for a scenario (P1) 🎯 MVP — PR 3

**Goal**: pick a scenario and see each rep's tier lines with FR citations.
**Independent test**: `Features/US1_TieredCommission.feature` passes; the page renders the
`tiers` scenario.

- [X] T027 [P] [US1] In `tests/CommissionCalculator.Specs/`, add step definitions that build
  `ScenarioInput` from Gherkin tables and assert, to the cent, both
  statement lines (description, amount, FR) — each description copied **exactly** from the
  Appendix A scenario that carries the example (e.g. "T-1 booked 2026-02-12 (closed 2026-02-10)",
  "5% of $80,000.00"), so T060's line-for-line check in Phase 9 can never disagree with an earlier
  feature; this applies to every story feature file (T037, T042, T046, T051, T055) — as an
  **ordered subsequence**: the lines a scenario
  names must appear in that order, and other lines may be present, because Phases 7–8 add draw,
  recovery and clawback lines to every statement and a test is never edited to pass (full-list
  equality is T060's job, in Phase 9) — and statement summary fields (`Attainment`,
  `CreditedBookings`, `ProratedQuota`, `Clawbacks`, `DrawPaid`, `Recovered`, `EarnedCommission`,
  `Payable`, `ClosingRecoverableBalance`). Write `Features/US1_TieredCommission.feature` with US1 AS1–AS4
  exact, including AS1's attainment of 80% (`@FR-006 @FR-007 @FR-009 @FR-018`). Run; record
  failing.
  **Result**: Recorded red 2026-09-22: US1 AS1–AS4 all failed on the line check ("Actual lines" held only the quota line). Green after T029.
- [X] T028 [P] [US1] Write `TierScheduleTests` (`FR-006`, `FR-007`): credit 80,000/100,000 → one 5%
  line 4,000.00; 160,000 → 5,000.00 / 4,000.00 / 1,200.00; exactly 150,000 → no 12% line;
  exactly 100,000 → no 8% line; 10.10 → 0.51; 150% edge from a rounded quota of 50,549.45 is
  75,824.18. Run; record failing.
  **Result**: Recorded red: 7/7 `TierScheduleTests` failed with `NotImplementedException`. Expected values cross-checked with an independent decimal calculation (edge 75,824.18; lines 2,527.47 / 2,021.98 / 501.10).
- [X] T029 [US1] Implement `Calculation/TierSchedule.cs` and crediting of single-rep deals in
  `Calculation/QuarterCredit.cs` wired into `StatementBuilder` (credit lines FR-008, tier lines
  FR-006, "Commission before refunds", attainment FR-009) until T027–T028 pass.
  **Result**: Done; engine 54/54 green. Phase 3 credits every deal of the rep (booking-date filtering is Phase 4); `EarnedCommission` equals commission before refunds until clawbacks exist (Phase 8).
- [X] T030 [P] [US1] Write `Features/UI_Page.feature` in Specs, driven through the web host
  (`@FR-001 @FR-002 @FR-003 @FR-021 @SC-002`): picker form structure per contracts/ui.md; exactly one
  `h1`; a skip link whose `href` targets the `<main>` element's id; on `?scenario=tiers` exactly four rep
  `section`s (one per A.1 rep) exist — so the check is red until T032 renders them — and each
  one's `aria-labelledby` names its `h2`'s id (*Guard*: drop the skip link's target id, and separately
  the section's `aria-labelledby`, and confirm each scenario fails; record). The `h1` and
  skip-link scenarios are expected **green** at write time, because T004 built the layout (with its
  `h1`) in
  Phase 1; their red evidence is the skip-link guard above and, for the `h1`, a guard that adds a
  second `h1` and confirms failure — record both;
  `?scenario=tiers` shows one section per rep with `h2`, caption, `th scope=col` Item/Amount/Rule;
  the page has at least one Rule cell and every Rule cell is an FR ID that exists in spec.md (read
  from the committed spec file) — so the check cannot pass on an empty page; each rep's
  page has a `<title>` naming the selected scenario (WCAG 2.4.2); these scenarios run against a scenario directory the scenario itself creates (a temp folder holding the Appendix A.1 inputs
  as JSON), never the shipped `Scenarios/` folder, which is only added in Phase 9 (T060a); summary `dl` shows quota, prorated quota, credited bookings, attainment (Avery: "80.00%"),
  earned, clawbacks, draws paid, draw recovered, payable and closing balance — for every A.1 rep,
  each `dl` value equals the matching `RepStatement` field from `CommissionEngine.Calculate` on the
  same input, formatted; each rep's table has
  exactly one row per engine `BreakdownLine`, in order, with the line's description, formatted
  amount and FR (compared with `CommissionEngine.Calculate` on the same A.1 input); US1 AS5 — with the A.1 and A.2 inputs both in the
  folder, selecting `booking-dates` shows only Emery and selecting `tiers` only Avery–Devon; `GET /`
  with no `scenario` shows the first scenario by file name; an empty scenario folder shows "No
  scenarios are installed" and the picker with no options; unknown id → 404 with the picker; a load error
  is shown — for this one scenario the host's scenario directory is a temp directory the scenario
  creates (one valid file, one malformed), never the shared build output. *Guard*: return 200 for an unknown id and confirm that scenario fails; record. Run;
  record failing. Add `AttainmentFormatTests` in Web.Tests (`FR-002`): 0.8 → "80.00%", 0.12345 →
  "12.35%" (midpoint, half away from zero), 1.6 → "160.00%". *Guard*: format with banker's rounding
  and confirm the midpoint case fails ("12.34%"); record. Add `MoneyFormatTests` in Web.Tests
  (`FR-021`, `FR-018`): 1234.5 → "$1,234.50", 0 → "$0.00", −1600 → "−$1,600.00" (minus sign in
  the text). *Guard*: format with `Math.Abs` and confirm the negative case fails; record. (T032's
  formatting is written against these, not against T057, which arrives in Phase 8.)
  **Result**: Recorded 2026-09-22: 13 of 16 host scenarios red on real assertions (e.g. picker "collection was empty", sections "Values differ"); the `h1` and skip-link scenarios green at write time, as declared. Format tests: 6/6 red (stubs). Guards: removing the main id failed the skip-link scenario; removing `aria-labelledby` failed the sections scenario; a second `h1` failed the h1 scenario; returning 200 for an unknown id failed the 404 scenario; banker's rounding failed the 12.35% case; dropping the minus sign failed the −$1,600.00 case.
- [X] T031 [P] [US1] Write `StylesheetAccessibilityTests` in Web.Tests (`FR-021`), reading
  `wwwroot/css/site.css`: 1.4.3 text contrast — body, table and link text on their backgrounds
  ≥ 4.5:1; 1.4.11 non-text contrast — focus indicator and `select`/`button` borders ≥ 3:1 against
  adjacent colours; 2.5.8 target size — `select`, `button` and the skip link have a minimum height
  and width of at least 24px; 2.4.11 focus not obscured — no `position: fixed` or `sticky` rules;
  1.4.12 text spacing — no fixed `height` and no `overflow: hidden` on text containers.
  *Guard*: separately (a) set the text token to a light grey, (b) set the button's min-height to
  16px, (c) add a `position: sticky` rule, (d) add a fixed `height` to the table caption; confirm
  each makes its test fail; record all four. Run: the 1.4.3, 1.4.11 and 2.5.8 checks are expected
  red (Phase 1's stylesheet defines no colour tokens or target sizes); the 2.4.11 and 1.4.12
  absence checks are expected **green** at write time, with guards (c) and (d) as their red
  evidence. Record which were red and which green.
  **Result**: Recorded: contrast (2 tests) and target-size (3 cases) red — no tokens or sizes existed; the two absence checks green at write time, as declared. Guards (a)–(d) each failed their test: light-grey text → contrast; 16px min-height → button and select target size; sticky rule → focus-not-obscured; fixed caption height → text spacing.
- [X] T032 [US1] Implement the page (picker, the empty state "No scenarios are installed",
  statements, summary `dl`, breakdown tables, `$#,##0.00` with a minus sign, `site.css` colour
  tokens and visible focus styles) until T030–T031 pass.
  **Result**: Done; Web 22/22 and Specs 19/19 green. A reflow defect found in T033 (summary amounts clipped at 320px) was fixed here: the summary's label column now shrinks and wraps.
- [X] T033 [US1] Keyboard-only check (FR-021, SC-005; standing rule 3): run the app with its
  scenario directory pointed at a folder holding the Appendix A.1 and A.2 inputs, and drive it with
  key presses only through the in-app browser (evidence: the action log contains no pointer
  events); Tab to the picker, change scenario with arrow keys, submit with Enter, Tab through each
  table; confirm visible focus at every stop. Then resize the viewport to 320 CSS px wide (1.4.10
  reflow; evidence: the viewport width reported by the browser just before the screenshot) and
  confirm no horizontal page scroll other than inside the breakdown tables. Record the log excerpt
  and screenshot in the PR.
  **Result**: Done 2026-09-22 in the in-app browser, key presses only (no pointer actions in the log). Verified: Tab reaches the skip link, then the scenario select, then Show, each with a visible focus outline; Enter on Show submits. **Not verified by the agent; confirmed by the maintainer in T034 (2026-09-22)**: changing the select's value by keyboard. ArrowDown, Down, typing "T" and Alt+ArrowDown all left the value unchanged while the select held focus, which points to the embedded browser's synthesized keys not driving the native select widget; this is handed to the maintainer with T034. Reflow (1.4.10) at a 320px viewport (`clientWidth` 320): first run found the summary amounts clipped at the right edge; after the T032 fix, no summary element extends past 320px (widest right edge 304px) and the page does not scroll horizontally.
- [X] T034 [US1] Screen-reader check (FR-021; standing rule 3) — **maintainer step**: with
  VoiceOver on (evidence: VoiceOver caption panel visible in a screenshot), navigate by headings
  and tables on `?scenario=tiers` (app run as in T033); confirm each rep's `h2`, the table caption and column headers are
  announced. The agent does not change system accessibility settings; the maintainer records the
  result on the PR.
- [X] T035 [US1] Add a second CI job, **offline smoke**: `dotnet publish` the web app, run it in
  `mcr.microsoft.com/dotnet/aspnet:10.0.12` with `docker run --network none`, and request `/` (HTTP
  200 with the picker; the seed-table assertion joins it in T060b) from a `curlimages/curl:8.22.0` container started with `--network container:<app>`
  (the runtime image has no HTTP client; research R16); as a negative control the same sidecar's
  request to an external host must fail (SC-004; evidence: `docker inspect` shows
  `NetworkMode: none`, printed immediately before the request). Both jobs write a CI-evidence record
  (`ci-evidence/<job>.json`: job name, check, requirement IDs `SC-004` and — for the smoke job —
  `FR-020`, result, commit SHA and run URL). These are CI gates, not tests; the trace lists them in
  their own "Verified by CI job" section and never counts them as tests (maintainer decision D1,
  2026-09-22). Each job uploads its `ci-evidence/*.json` with `actions/upload-artifact@v7` so the
  traceability job (T061) can download it. *Guard*: point the sidecar's request at a path
  that returns 404 and confirm the job fails; *Guard (no network)*: run the job once with the app
  container on the default bridge network instead of `--network none` and confirm the job fails
  because the external request succeeds — the positive control for the "no network" evidence;
  record both.
  **Result**: Done; offline-smoke job and CI-evidence records added. Local run 2026-09-22 (Docker 29.8.0): `NetworkMode: none`, GET / → 200, external request failed as required. Guards: GET /does-not-exist → "404, expected 200", exit 1; `bridge` network → "an external request succeeded", exit 1.
- [X] T036 [US1] Checkpoint: PR "Phase 3: US1", CI green, maintainer approves squash-merge. Record
  the offline job's first run in research R16 (Docker on the runner) and confirm the plan's
  Provenance gate; propose to the maintainer a PATCH amendment adding Docker (a CI-only dependency
  of the offline-smoke job) to constitution C7 and its pairwise note.

  **Result**: CI green on PR #5 (run 35762826388: build-test, smoke, offline-smoke). R16 confirmed and the
  plan's Provenance gate updated; the Docker PATCH amendment is proposed to the maintainer at this gate.
---

## Phase 4: User Story 2 — booking date decides the quarter (P1) — PR 4

**Goal**: only deals booked in the quarter count; excluded deals are listed with the reason.
**Independent test**: `Features/US2_BookingDate.feature` passes.

- [X] T037 [P] [US2] Write `Features/US2_BookingDate.feature`: US2 AS1–AS4 with exact dates and
  amounts (`@FR-008`), including the excluded-deal line citing FR-008. Run; record failing —
  expected red: AS1 (booked 2026-04-02, after the quarter) and AS4 (both dates outside), because
  Phase 3's T029 credits every deal of the rep, so they are still counted. AS2 (closed 2025-12-29,
  booked 2026-01-05) and AS3 (both dates inside) are expected **green** at write time, because
  Phase 3 already credits them. Their red evidence: AS2 — T038's close-date guard makes it fail;
  AS3 — *Guard*: make crediting exclude every deal and confirm AS3 fails; record. Record which
  scenarios were red and which green at write time (labels checked against spec.md US2 AS1–AS4,
  2026-09-22).
  **Result**: Recorded 2026-09-22: AS1 and AS4 red (Phase 3 still counted the excluded deals); AS2 and AS3 green at write time, as declared. Red evidence: close-date crediting failed AS1 and AS2; excluding every deal failed AS3.
- [X] T038 [P] [US2] Write `QuarterCreditTests` (`FR-008`): booked the day before/after the quarter
  is excluded (expected red: Phase 3 credits every deal); booked on the first and last day counts,
  and close date never changes the result (expected green at write time — Phase 3 already credits
  them; red evidence is the guards below plus an inclusive-bounds guard: make the bounds exclusive
  and confirm the first/last-day tests fail). Add `ScenarioValidatorTests` (`FR-004`) for the two
  refund rules this phase introduces, each in both the scenario's deal list and a booking-quarter
  deal list: refund dated before the deal's booking date, and refunds totalling more than the deal
  — each rejected. (Refund ≤ 0 and sub-cent refunds are T018's shared rules.) *Guard*: switch
  crediting to close date and confirm AS1 (B-1) and AS2 (B-2) fail; remove each refund check and
  confirm its tests fail; validate refunds only on the scenario's own list and confirm the
  booking-quarter cases fail; record. Run; record failing.
  **Result**: Recorded: the 6 `QuarterCreditTests` were red at write time from the new quarter-aware overload's stub, including the three declared green. That declaration assumed they would run against Phase 3's crediting, but they call the new overload. The 4 new refund-rule cases were red on the real assertion. Guards: close-date crediting failed the day-before, day-after and close-date tests; exclusive bounds failed the first-day and last-day tests; removing the refund-date check failed its 2 cases; removing the refund-total check failed its 2 cases; checking refunds only on the scenario's own list failed all 4 booking-quarter refund cases.
- [X] T039 (Moved into T030 by analyze, 2026-09-22: US1 AS5 asserts only which reps a scenario shows,
  which Phase 3 already renders, so it could not be seen failing here.)
- [X] T040 [US2] Implement booking-date crediting and excluded-deal lines until T037–T038 pass.
  **Result**: Done; 115/115 green locally, engine coverage 98.09%, every test passes alone. Excluded deals appear in deal-list order with Appendix A.2's wording.
  **Result**: Maintainer confirmed 2026-09-22 (at the Phase 7 gate): "T034 is complete and working
  as intended" — the VoiceOver pass over `?scenario=tiers`, and with it the keyboard behaviour of
  the scenario select that T033 could not drive through the in-app browser. T033's unverified item
  is therefore closed by this check, not by the agent.
- [ ] T041 [US2] Checkpoint: PR "Phase 4: US2", CI green, maintainer approves squash-merge.

---

## Phase 5: User Story 3 — prorated quota (P2) — PR 5

**Goal**: mid-quarter starters are measured against a calendar-day-prorated quota.
**Independent test**: `Features/US3_Proration.feature` passes.

- [X] T042 [P] [US3] Write `Features/US3_Proration.feature`: US3 AS1–AS4 exact, including the
  prorated-quota summary values (`@FR-010 @FR-009 @FR-018`). Run; record failing — AS1–AS3
  expected red; AS4 (start on or before the first day: no proration, no prorated-quota line) is
  expected **green** at write time, because nothing prorates before T044. *Guard*: prorate
  unconditionally and confirm AS4 fails; record.
  **Result**: Recorded 2026-09-22: US3 AS1–AS3 red, AS4 green at write time, as declared. Guard: prorating unconditionally failed AS4 (and the US1 scenarios).
- [X] T043 [P] [US3] Write `QuotaProrationTests` (`FR-010`, `FR-004`, `FR-011`): 45/90 of
  90,000.00 = 45,000.00; 46/91 of 100,000.00 = 50,549.45; start on or before the first day → no
  proration (asserted on `QuotaProration`'s result, which is a stub until T044, so red); start on
  the last day → 1 day. Validator rules this phase introduces, each in every place it applies: a
  prorated quota rounding to 0.00 → rejected (scenario roster and booking-quarter reps); start
  after quarter end → rejected naming the rep; any deal (counted or not, in the scenario's list or
  a booking quarter's) booked before the start date of a credited rep or partner → rejected
  naming deal and rep. *Guard*: remove each rejection check and confirm its tests fail; apply them
  only to the scenario's own data and confirm the booking-quarter cases fail; record. Run; record
  failing.
  **Result**: Recorded: 5 `QuotaProrationTests` red from the stub; the 6 new validator cases red on the real assertion. Guards: removing the start-after-quarter-end check, the zero-prorated-quota check and the booked-before-start check each failed their 2 cases; applying the start-date rules to the scenario's own data only failed all 3 booking-quarter cases.
- [X] T044 [US3] Implement `Calculation/QuotaProration.cs` and the FR-011/FR-004 start-date rules
  (a split partner with no start date is skipped by the FR-011 check until T058 adds its own
  rejection rule, so T056's test for that rule is red for the reason it states)
  until T042–T043 pass.
  **Result**: Done; 124/124 green. The duplicate-repId test caught a real defect in the first implementation: building the start-date lookup with `ToDictionary` threw on a duplicate id, which the contract forbids (the engine never throws for invalid data). The lookup now keeps the earliest start date per id.
- [ ] T045 [US3] Checkpoint: PR "Phase 5: US3", CI green, maintainer approves squash-merge.

---

## Phase 6: User Story 4 — split deals (P2) — PR 6

**Goal**: split shares by largest remainder, credited toward attainment and commission.
**Independent test**: `Features/US4_Splits.feature` passes.

- [X] T046 [P] [US4] Write `Features/US4_Splits.feature`: US4 AS1–AS5 exact, including AS2's
  credited bookings of $110,000.00 and attainment of 110% (`@FR-012 @FR-013 @FR-009`). Run; record
  failing.
  **Result**: Recorded red 2026-09-22: US4 AS1–AS5 all failed (no split crediting, no FR-013 rule). Green after T049.
- [X] T047 [P] [US4] Write `SplitAllocationTests` (`FR-012`): 60/40 of 50,000.00; 50/50 of 10.01 →
  5.01/5.00; 33.335/33.335/33.33 of 100.00 → 33.34/33.33/33.33; 45/45/10 of 0.06 → 0.03/0.03/0.00;
  45/45/10 of 0.05 → 0.02/0.02/0.01 (US6 AS9's re-split); shares always sum to the amount over a
  fixed table of cases. Add validator tests (`FR-013`), each in the scenario's deal list and a booking-quarter deal list
  (standing rule 8): sums of 99.999 and 100.001 rejected
  (expected red); 100 accepted (expected **green** at write time — nothing rejects it yet;
  *Guard*: make the sum check reject every deal and confirm it fails). *Guard*: replace largest remainder with independent rounding and confirm the 10.01
  case fails; remove the sum check and confirm the FR-013 tests fail; apply the sum check only to
  the scenario's own deal list and confirm the booking-quarter cases fail; record. Run; record
  failing.
  **Result**: Recorded red: 6 `SplitAllocationTests` (stub) and the 4 FR-013 cases; "sum exactly 100 accepted" green at write time, as declared. Guards: independent rounding failed 5 allocation cases including the 10.01 split; removing the sum check failed its 4 cases; making the sum check reject every deal failed the accepted case (its red evidence); applying the sum check to the scenario's own deal list only failed all booking-quarter split cases. A theory's `InlineData` ints did not bind to `params double[]` ("arguments did not match the parameters") and were written as doubles.
- [X] T048 [P] [US4] Write `Features/UI_RejectedScenario.feature` in Specs, driven through the web
  host (`@FR-004`), against a scenario directory the scenario itself creates (a temp folder holding the Appendix A.11 inputs
  as JSON), never the shipped `Scenarios/` folder, which is only added in Phase 9 (T060a): `?scenario=invalid` renders an element with `role="alert"` listing four errors,
  each with its FR ID, and no rep table. *Guard*: stop rendering the alert's error list and confirm
  the scenario fails; record. ("No rep table" needs no separate guard: `RejectedScenario` carries
  no statements, and T020 guards the engine side.) Run;
  record failing.
  **Result**: Recorded red: the rejected-scenario page scenario failed (no alert). Guard: not rendering the alert's error list failed it. The A.11 fixture was regenerated after a JSON generator bug wrote `100000.0.00`, which the reader correctly rejected as a load error.
- [X] T049 [US4] Implement `Calculation/SplitAllocation.cs`, split crediting in `QuarterCredit`
  (share lines cite FR-012), the FR-013 rule and the rejected-scenario view until T046–T048 pass.
  **Result**: Done; 146/146 green. Per-rule requirement ids added so each error cites its own FR, as Appendix A.11 states them.
- [ ] T050 [US4] Checkpoint: PR "Phase 6: US4", CI green, maintainer approves squash-merge.

---

## Phase 7: User Story 5 — monthly draw and recovery (P2) — PR 7

**Goal**: draws per month (start month prorated), recovery, payable, carried balance.
**Independent test**: `Features/US5_Draw.feature` passes.

- [X] T051 [P] [US5] Write `Features/US5_Draw.feature`: US5 AS1–AS6 exact, including AS6's total
  draw of $9,548.39 (`@FR-014 @FR-015`). Run; record failing.
  **Result**: Recorded red 2026-09-22: US5 AS1–AS6 (5 scenarios) all failed — no draw or recovery lines existed. Green after T053.
- [X] T052 [P] [US5] Write `DrawScheduleTests` (`FR-014`): full quarter = 3 × 4,000.00; start
  2026-02-15 → 0.00 / 2,000.00 / 4,000.00; start 2026-01-20 → 1,548.39; start on the 1st of the
  second month → 0.00 / 4,000.00 / 4,000.00. Write `DrawRecoveryTests` (`FR-015`): earned ≥
  recoverable total; earned < total; earned = 0; earned negative (recovered 0.00, payable 0.00,
  closing = total + |earned|); opening balance included; `PayableIsNeverNegative` over those cases.
  *Guard*: remove the negative-earned branch — expected result: recovered becomes −1,600.00 in the
  negative case, so that test fails; record. There is no separate payable clamp to remove (payable
  is `earned − min(earned, total)` ≥ 0 when earned ≥ 0); `PayableIsNeverNegative` is therefore a
  property check, not a failure-path guard — recorded here per standing rule 2. Run; record
  failing.
  **Result**: Recorded red: 4 `DrawScheduleTests` and 11 `DrawRecoveryTests` cases failed with `NotImplementedException`. Guards: removing the negative-earned branch failed `NegativeEarningsAreAddedToTheBalanceAndRecoverNothing` (recovered became −1,600.00, as predicted); prorating every month, not only the start month, failed all 4 draw-schedule tests. `PayableIsNeverNegative` is a property check with no guard, as the task records.
- [X] T053 [US5] Implement `Calculation/DrawSchedule.cs` and `Calculation/DrawRecovery.cs` and
  their statement lines until T051–T052 pass.
  **Result**: Done; 172/172 green, engine coverage 98.16%, every test passes alone. The "Earned commission" line (FR-015) is added here; the "Clawbacks" line above it is Phase 8's.
- [ ] T054 [US5] Checkpoint: PR "Phase 7: US5", CI green, maintainer approves squash-merge.

---

## Phase 8: User Story 6 — clawback on refunds (P3) — PR 8

**Goal**: clawbacks per refund, in the refund's quarter, sized by recomputing the booking quarter.
**Independent test**: `Features/US6_Clawback.feature` passes.

- [X] T055 [P] [US6] Write `Features/US6_Clawback.feature`: US6 AS1–AS9 exact, including the
  re-split lines (FR-017) — AS7's $4,950.10 / $4,950.09 and AS9's $0.01 — (`@FR-016 @FR-017`), and
  the Q2 scenarios with booking-quarter data. Run; record failing — AS3 (refund dated after the
  quarter: the deal counts, no clawback) is expected **green** at write time, because no clawback
  exists before T058; its red evidence is T056 guard (b). All other scenarios expected red.
  **Result**: Recorded red 2026-09-22: 8 of 9 US6 scenarios failed; AS3 (refund dated after the quarter) green at write time, as declared, with T056 guard (b) as its red evidence.
- [X] T056 [P] [US6] Write `ClawbackCalculatorTests` (`FR-016`, `FR-017`): full, partial and
  repeated refunds; refunds across deals ordered by date then deal then refund position; refund
  after quarter end ignored; negative clawback from a re-split; split refund re-split
  (4,950.10 / 4,950.09). Add validator tests (`FR-004`) for the rules only booking-quarter data
  has (every rule shared with the scenario's own data is already tested in T018, T038, T043 or
  T047): a deal booked in an earlier quarter and refunded in this one, with no booking-quarter data
  for that quarter → rejected; a roster rep credited on such a deal with no entry in that booking
  quarter's `reps` → rejected; a split partner on **any** booking-quarter deal who is neither on the
  roster nor in `partners` with a start date → rejected (FR-011's start-date check needs that date) (the detectable completeness rules, clarification
  2026-09-22); booking quarter overlapping the scenario quarter → rejected; a booking-quarter deal
  whose booking date is outside that quarter's dates → rejected; a rep's start date in booking-
  quarter data differing from the roster's → rejected; a deal in both lists differing → rejected;
  a partner listed with a start date → accepted (expected **green** at write time; *Guard*: reject
  every booking-quarter partner and confirm it fails). Add a statement test: a refund on a split
  deal adds a re-split line citing FR-017 before its clawback line; a refund on a single-rep deal
  adds none (expected **green** at write time — no re-split lines exist before T058; *Guard*: emit
  a re-split line for every refund and confirm it fails; record).
  *Guard*: (a) size each refund against the untouched quarter and confirm AS6 fails; (b) remove
  the refund-date filter and confirm AS3 and AS5 (Q1) fail; (c) floor clawbacks at zero and
  confirm AS9 fails; (d) remove each validation check listed above and confirm its test fails;
  record all. Run; record failing.
  **Result**: Recorded red: 8 `ClawbackCalculatorTests`, 7 booking-quarter validator cases and the re-split statement test. "A partner listed with a start date is accepted" and "a refund on a single-rep deal adds no re-split line" green at write time, as declared. Guards: (a) sizing each refund against the untouched quarter failed AS6; (b) removing the refund-date filter failed AS3 and AS5; (c) flooring clawbacks at zero failed AS9; (d) emitting a re-split line for every refund failed the single-rep test, and rejecting every booking-quarter partner failed the accepted case and the valid-scenario baseline.
- [X] T057 [P] [US6] Write `Features/UI_NegativeAmounts.feature` in Specs, driven through the web
  host (`@FR-021 @FR-015`): `?scenario=refunds-q2` renders Sage's earned commission as "−$1,600.00"
  with a minus sign in the text (not colour alone), against a scenario directory the scenario itself creates (a temp folder holding the Appendix A.10 inputs
  as JSON), never the shipped `Scenarios/` folder, which is only added in Phase 9 (T060a). Its
  first red is recorded with its actual cause (no clawback line yet, so earned is not negative);
  the formatting itself was test-driven in T030's `MoneyFormatTests`. *Guard*: format with `Math.Abs` and confirm the test fails; record.
  Run; record failing.
  **Result**: Recorded red: the negative-amount page scenario failed — its first red was the missing clawback (earned was not negative yet), as the task states. The minus-sign formatting itself was test-driven in T030.
- [X] T058 [US6] Implement `Calculation/ClawbackCalculator.cs`, refund-aware `QuarterCredit`,
  booking-quarter validation and clawback lines until T055–T057 pass.
  **Result**: Done; 200/200 green, engine coverage 98.63%, every test passes alone. One message was reworded (not the test) when a date interpolated into the middle of the phrase a test matched on.
- [ ] T059 [US6] Checkpoint: PR "Phase 8: US6", CI green, maintainer approves squash-merge.

---

## Phase 9: Polish & cross-cutting — PR 9

- [ ] T060 [P] Write `Features/SeededScenarios.feature` (`@SC-001 @SC-002 @SC-003`): for every file in
  `Scenarios/`, load it through the real reader and assert every rep's statement equals Appendix A
  line for line (description, amount to the cent, FR), and that each `RepStatement` summary field
  equals its Appendix A line (Quarterly quota → `Quota`; Prorated quota, or the quota when there is
  none → `ProratedQuota`; Credited bookings; Commission before refunds; Clawbacks; Earned
  commission; Draws paid → `DrawPaid`; Draw recovered → `Recovered`; Commission payable →
  `Payable`; Closing recoverable balance), and `invalid.json` lists exactly the four
  Appendix A.11 errors. SC-003 check: the brief's rules map to FRs as rule 1 → FR-005, rule 2 →
  FR-006, rule 3 → FR-010, rule 4 → FR-012, rule 5 → FR-014/FR-015, rule 6 → FR-016, rule 7 →
  FR-008, rule 8 → FR-019. Each rule is a check function over a set of loaded statements:
  rule 1 — the seeded reps do not all have the same quota; rule 2 — lines at 5%, 8% and 12%;
  rule 3 — a "Prorated quota" line (FR-010); rule 4 — a share line (FR-012); rule 5 — a non-zero
  "Draw recovered" and a non-zero "Commission payable"; rule 6 — a non-zero clawback line
  (FR-016); rule 7 — a line with section `Excluded` citing FR-008 (every statement has an FR-008
  "Credited bookings" line, so a plain FR-008 match could not fail); rule 8 — every seeded file
  loads under the strict reader (no currency, tax or term fields exist to set) and every rendered
  amount is in dollars, `$` or `−$` (a second permanent scenario feeds the dollar check an amount
  formatted without `$` and expects it to fail — it loads no seed file, so it is expected **green**
  at write time; *Guard*: make the dollar check accept any string and confirm it fails; record). Every breakdown line's FR exists in spec.md (SC-002).
  Each check is shown able to fail by a **permanent** scenario that runs it over the seed set
  with *every* scenario carrying that rule removed (carriers computed from Appendix A,
  2026-09-22) and expects that check — not file loading — to report failure: rule 2 without
  {tiers, draw} (the only 12% lines); rule 3 without {proration, proration-q2}; rule 4 without
  {splits, split-rounding, refund-splits}; rule 5 without {draw} (the only non-zero payable,
  Parker); rule 6 without {refunds, refund-splits, refunds-q2}; rule 7 without {booking-dates};
  rule 1 with the `proration` quotas replaced by $100,000.00 in memory; rule 8 with a
  `"currency": "EUR"` property added to one seed's JSON in memory — for rule 8 the check *is* the
  strict reader, so its expected failure is that load error. The feature
  lists the eleven expected files by name, so before T060a it fails on every one ("scenario file
  not found"); run it and record that red. Any later failure is a spec question, not a test edit.
- [ ] T060a Add the eleven seed files `src/CommissionCalculator.Web/Scenarios/{tiers,
  booking-dates, proration, proration-q2, splits, split-rounding, draw, refunds, refund-splits,
  refunds-q2, invalid}.json`, transcribing Appendix A.1–A.11 inputs, until T060 passes. A mismatch
  is fixed in the seed file, never in Appendix A or the test.
- [ ] T060b Extend the CI smoke job (T010) and the offline-smoke job (T035): request
  `/?scenario=tiers` and require a `<table>` containing "Avery" — proving the running app loaded the
  seed files from its output. *Guard*: on a throwaway branch add `<Content Update="Scenarios/*.json"
  CopyToOutputDirectory="Never" CopyToPublishDirectory="Never" />` and confirm both assertions fail
  (research R17 observed that this removes the files from output); record.
- [X] T061 Traceability generator, test first: add `TraceabilityTests` to Tools.Tests
  (`[Trait("Principle", "III")]`; fixture spec with FR-001..FR-003 and SC-001, fixture TRX, fixture
  assembly metadata, plus one case that runs the built tools executable **as a separate process**
  against a fixture DLL built from a committed fixture project,
  `tools/CommissionCalculator.Tools.Fixture/` (Web SDK with `<OutputType>Library</OutputType>` —
  the Web SDK's default Exe fails with CS5001 — one Razor Pages `PageModel` marked
  `[Implements("FR-001")]`, referencing the Engine for `ImplementsAttribute`; outside `tests/`, so
  T001's test-package rule does not make it a test application, and not in
  `CommissionCalculator.slnx`, so `dotnet test` never runs it); Tools.Tests builds it
  through a `ProjectReference` with `ReferenceOutputAssembly="false"` and loads it from the
  fixture's output path in the child process, so no ASP.NET framework reference reaches the
  tools process through the test. `trace` recognises the attribute by its full type name,
  `CommissionCalculator.Engine.ImplementsAttribute`, via `CustomAttributeData`
  and expects its FR in the output — *Guard*: remove the framework reference from the tools project
  and confirm that case fails with `ReflectionTypeLoadException` in the child process's output;
  record) — an ID with no test and an ID with no member both appear under "Gaps"; an ID
  declared in the explained-gaps table (below) still appears under "Gaps" — Principle III requires
  every FR/SC lacking a test or a member to be listed as a gap — but in an "Explained" subsection
  with its reason and category (scope, evidence-only, CI evidence, manual evidence), while any gap
  without an entry appears under "Unexplained"; tests carrying a `Principle` trait appear under "Tooling tests" and not against any FR; a
  CI-evidence record (`ci-evidence/*.json`) appears under "Verified by CI job" with its job, result
  and run URL, is never counted as a test, and a failed record makes its gap Unexplained.
  *Guard*: disable gap detection and confirm the gaps test fails; record. Run; record failing. Then add the
  `trace` command to `tools/CommissionCalculator.Tools` (FR/SC IDs from spec.md; test → IDs from
  TRX categories/traits; member → IDs from `[Implements]` via reflection over the built engine and
  web assemblies; explained-gaps table — scope: FR-019, "USD only; tax, currency conversion and
  multi-year are out of scope, so no member implements them"; evidence-only (tests, no single
  member): SC-001 — seed files and Appendix A, SC-002 — every line cites an FR (T030, T060),
  SC-003 — the seeded scenarios (T060); CI evidence (no test): SC-004 — the smoke job and
  offline-smoke job (T010, T035, T060b); manual evidence (no test the trace can see): SC-005 —
  T033/T064. The table has entries only for IDs that are gaps; FR-021 has tests and members and is
  not a gap. Its screen-reader clause is covered by T034's manual check, which the report lists as
  a note under SC-005's entry, not as a gap) writing
  `specs/001-commission-calculator/traceability.md`. Add a final CI job, `traceability`, that
  depends on the build/test job, the smoke job (T010) and the offline-smoke job
  (T035), downloads their artifacts with `actions/download-artifact@v8` (suite TRX files,
  `ci-evidence/*.json`, and the build/test job's uploaded Debug build output
  `src/CommissionCalculator.{Engine,Web}/bin/Debug/net10.0/`, the configuration `dotnet build` and
  `dotnet test` produce), runs `trace --assemblies
  <engine.dll> <web.dll> --results <files>` over them and uploads the result. The build/test job
  (T007) gains an upload of that build output.

  **Result**: Done. `TraceabilityTests` (11 cases, `[Trait("Principle", "III")]`) written first and run
  red 2026-09-22 (11 failed / 6 passed: every new case failed on `NotImplementedException`, and the
  child-process case on the tools usage message, `trace` not yet being a command). Fixture project
  `tools/CommissionCalculator.Tools.Fixture/` (Web SDK, `<OutputType>Library</OutputType>`, one
  `[Implements("FR-001")]` Razor Pages model), referenced with `ReferenceOutputAssembly="false"`;
  the in-process member test uses an `ImplementsAttribute` declared in the test assembly under the
  engine's namespace, so full-type-name matching is what is proved. Implemented `Traceability` and
  `Trace`; 17/17 Tools.Tests green. *Guards*: gap detection disabled (`&& DateTime.Now.Year < 0`,
  because `if (false)` breaks the build under warnings-as-errors) — the five gap cases failed;
  `FrameworkReference Include="Microsoft.AspNetCore.App"` removed from the tools project — the
  child-process case failed with `Unhandled exception. System.Reflection.ReflectionTypeLoadException`
  in the captured output. Both restored (`grep -c` 0 and 1 respectively) and green again. CI job
  `traceability` added (needs build-test, smoke, offline-smoke; `actions/download-artifact@v8`;
  `--strict`), and build-test now uploads the Debug build output.
- [X] T062 Add `[Implements]` to every engine and web member that implements an FR; run `trace`
  with the same assemblies and result files the CI job uses; the report's "Unexplained" gaps are
  empty, and its "Explained" gaps are exactly the explained-gaps table of T061, with SC-004's CI
  record passing and the manual-evidence items carrying the PR links recorded in T033/T034 (the
  T064 re-check links are added to the report when T065's PR is opened).

  **Result**: Done 2026-09-22. The first local trace reported three unexplained gaps: FR-011 (no test,
  no member), FR-013 (no member), FR-020 (no member). Fixed at the source, not in the report:
  `[Implements("FR-011")]` on `ScenarioValidator.StartDateFailure`, `[Implements("FR-013")]` on
  `SplitSumFailure`, `[Implements("FR-020")]` on the web `Program` composition root; and the
  validator rule theory, whose cases cover FR-011's start-date rules as well as FR-004's, now
  carries both `Requirement` traits. Second run: **0 unexplained gaps**, `--strict` exit 0, and the
  Explained table is exactly T061's six entries (FR-019 scope; SC-001..SC-003 evidence-only;
  SC-004 CI evidence, record passing; SC-005 manual evidence with the T033/T034 PR links) plus the
  SC-005 note on FR-021's screen-reader clause. 232/232 tests green. The committed report is
  regenerated from the CI artifacts at T065, so its CI-evidence rows carry the real run URL.
- [ ] T063 Re-verify every failure-path guard added in T007 (warnings, coverage), T008, T009,
  T010, T018, T020, T022 (error capture, unknown property, required field), T023 (first-failure, missing directory,
  duplicate id), T030 (unknown id, skip-link
  target, second `h1`, section labelling, attainment format, money format), T031 (all four),
  T035 (404, no-network), T037 (AS3 exclude-all), T038 (close date, bounds, refund date, refund total, own-list-only), T042 (AS4), T043,
  T047 (largest remainder, sum check, sum-100 accepted, own-list-only), T048, T052, T055 (AS3), T056 (incl. partner accepted, detectable completeness,
  overlap, date range, start-date mismatch, differing duplicate, single-rep no re-split), T061 (framework reference), T057, T060 (every rule check, dollar check), T060b, T061 — each still fails with its guarded
  behaviour removed; record each result here.
- [ ] T064 Quickstart validation (standing rule 3): step 1 from a fresh clone (evidence:
  `git status --ignored` shows no build output before running); re-run T033's keyboard check on the
  final UI; the maintainer re-runs T034's VoiceOver check. Record results in the PR.
- [ ] T065 Checkpoint: PR "Phase 9: Polish", CI green, maintainer approves squash-merge.

---

## Dependencies & execution order

- Phases run strictly in order 1 → 9; each starts only after the previous phase's PR is merged
  (standing rule 4). Stories are sequential for one reviewer even where independent.
- Within a phase: test tasks first (seen failing), then implementation; `[P]` test tasks can be
  written together.
- US1 (MVP) delivers the page and tier lines with FR citations, verified against test-created
  scenario folders; the shipped app shows "No scenarios are installed" until Phase 9.
- US2–US6 each add their rules. Polish writes the full-statement check (SC-001) first, sees it fail,
  then adds the eleven seed files it checks (T060, T060a) — so seed contents are test-driven.

## Parallel examples

- Phase 3: T027, T028, T030 and T031 are different files — write them together, run, see all fail.
- Phase 8: T055, T056 and T057 together; T058 afterwards.

## Implementation strategy

MVP first (Phases 1–3), then one story per PR in priority order, each independently testable
through its feature file. Each PR description lists the tests seen failing and the guards'
observed results.
