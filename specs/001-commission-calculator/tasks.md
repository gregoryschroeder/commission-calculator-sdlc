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
   app loads them. Before Phase 9, host-level tests use scenario folders they create themselves. It adds no environment variables, secrets, services or
   configuration.
6. **Every test names what it verifies**: `[Trait("Requirement", "FR-0xx")]` on unit tests,
   `@FR-0xx` / `@SC-00x` tags on Gherkin scenarios; tests of project tooling carry
   `[Trait("Principle", "II")]` / `("III")` for the principle they enforce (constitution v1.1.0).
   Tests that exercise the running web host are integration tests and are Gherkin scenarios in the
   Specs project; only single-class checks (file reader, catalog, stylesheet contrast) are xUnit. Every acceptance example is tested with its
   exact inputs and asserted to the cent — breakdown lines *and* statement summary values
   (attainment, credited bookings, total draw, re-split shares). A test is never edited to make it
   pass; a test that looks wrong is a spec question for the maintainer.

---

## Phase 1: Setup (shared infrastructure) — PR 1

**Purpose**: solution skeleton, build rules, CI helper tools and the CI review gate.

- [ ] T001 Create `global.json` (SDK 10.0.400, `rollForward: latestFeature`, test runner
  `Microsoft.Testing.Platform`), `Directory.Build.props` (net10.0, nullable, implicit usings,
  `TreatWarningsAsErrors`, `AnalysisLevel` latest-recommended, `EnforceCodeStyleInBuild`),
  `Directory.Packages.props` (central versions per plan.md), `.gitignore`, and
  `CommissionCalculator.slnx` — structural, no test.
- [ ] T002 Create `src/CommissionCalculator.Engine/CommissionCalculator.Engine.csproj` (no package
  references) and `src/CommissionCalculator.Web/CommissionCalculator.Web.csproj` (Razor Pages,
  references Engine only; `public partial class Program` for tests) — structural, no test.
- [ ] T003 Create `tests/CommissionCalculator.Specs/` (Reqnroll.xUnit.v3, Mvc.Testing, AngleSharp,
  TrxReport, CodeCoverage) with a `WebApplicationFactory<Program>` hook, and write the first
  scenario **before** the page exists: `Features/AppHost.feature` (`@FR-020`) — "the app serves its
  page": GET `/` returns 200 with `<html lang="en">` and one `<main>`. Run it; record it failing.
- [ ] T004 Implement the minimal `Program.cs` and `Pages/Index.cshtml` (layout with `lang`, skip
  link, `<main>`, local `wwwroot/css/site.css`) until T003 passes. (`Web.Tests` is created in T022
  together with its first tests: a test project with no tests makes `dotnet test` exit non-zero.)
- [ ] T005 Create `tests/CommissionCalculator.Tools.Tests/` and write tests for the CI helper
  commands **before** they exist (`[Trait("Principle", "II")]`) (fixture files under `tests/CommissionCalculator.Tools.Tests/
  Fixtures/`): `CoverageGateTests` — engine line rate 0.85 passes, 0.79 fails, report with no
  engine package fails *unless* the engine assembly contains no types (then passes with the message
  "no engine lines yet"), using a fixture report that *has* an engine package so the floor can
  bite; `TrxTestListTests` — lists fully qualified `className.name` for xUnit and
  Reqnroll tests from a fixture TRX (the Reqnroll display-name case from research R15). Run; record
  failing.
- [ ] T006 Create `tools/CommissionCalculator.Tools/` (console app; commands `coverage-gate` and
  `list-tests`) until T005 passes. (Replaces research R8's single-file program: a project can be
  unit-tested; see research R8 correction.)
- [ ] T007 Write `.github/workflows/ci.yml`: checkout@v7, setup-dotnet@v6 from `global.json`,
  `dotnet build -warnaserror`, `dotnet test --fail-skips on --report-trx --coverage
  --coverage-output-format cobertura`, `coverage-gate` on the engine (floor 80%, rate written to
  the job summary), upload TRX and coverage as artifacts.
  *Guard (warnings)*: on a local throwaway change add an unused variable in Engine; confirm the
  build fails; revert; record. *Guard (coverage)*: run `coverage-gate` with floor 101% against the
  Tools.Tests fixture report that contains an engine package (the real report has no engine lines
  until Phase 2); confirm non-zero exit; record. Repeat against the real report in T063.
- [ ] T008 CI per-test isolation step: for **every** test in all test projects (list from the suite
  TRX via `list-tests`), run `dotnet test --project <proj> --filter-method <className.name>` alone;
  fail if any fails. *Guard*: on a local throwaway branch add two Tools tests where one passes only
  when the other ran first (static flag); confirm the suite passes and this step fails; delete it;
  record. Record the step's CI duration in research R15 on the first run.
- [ ] T009 *Guard (skip gate)*: on a local throwaway change add `[Fact(Skip="probe")]`, run the CI
  test command; confirm a non-zero exit ("Failed!"); remove it; record.
- [ ] T010 CI smoke step: from the fresh checkout, start `dotnet run --project
  src/CommissionCalculator.Web` in the background, poll `/` until HTTP 200 (fail after 60 s), stop
  it. *Guard*: point the poll at a path that returns 404 and confirm the step fails; record.
- [ ] T011 `README.md`: what it is, `dotnet run --project src/CommissionCalculator.Web` (or
  `dotnet run` inside that folder), test commands, where the spec lives.
- [ ] T012 Checkpoint: open PR "Phase 1: Setup", CI green, maintainer approves squash-merge. Record
  the first CI run's outcome in research R14 (SDK resolution) and R15 (isolation-step time), and
  propose to the maintainer the PATCH amendment that marks constitution C7 as confirmed.

---

## Phase 2: Foundational (blocking prerequisites) — PR 2

**Purpose**: engine contract types, money rules, the engine entry point and the scenario-file
reader that every story uses.

- [ ] T013 Create the engine's public input/result records and `ImplementsAttribute` exactly as in
  contracts/engine-api.md (`Inputs/`, `Results/`) — structural, no test.
- [ ] T014 Create `tests/CommissionCalculator.Engine.Tests/` and write `MoneyTests`
  (`FR-018`, `FR-004`): half away from zero (0.505→0.51, −0.505→−0.51, 1548.387…→1548.39,
  50549.4505…→50549.45); whole-cent check true for 10.10, false for 10.005. Run; record failing.
- [ ] T015 Implement `Calculation/Money.cs` until T014 passes.
- [ ] T016 Write `QuarterTests` (`FR-004`, `FR-014`): 2026-01-01..03-31 has 90 days and months
  Jan/Feb/Mar; 2026-04-01..06-30 has 91; a quarter not starting on the 1st or not spanning three
  whole months is invalid. Run; record failing.
- [ ] T017 Implement quarter day/month helpers (internal, in `Calculation/`) until T016 passes.
- [ ] T018 Write `ScenarioValidatorTests` for the shared rules (`FR-004`): quota ≤ 0, deal amount
  ≤ 0, sub-cent money, negative opening balance, empty roster, bad quarter shape, deal credited to
  a rep not on the roster, same rep twice on a deal, split % ≤ 0 or > 100 — each test asserts
  that its own error is among those listed and the scenario is rejected (not an exact error set:
  later phases add rules such as FR-013 that a > 100% split also trips, and a test is never edited
  to pass); one further test with three mutually independent violations (quota ≤ 0, sub-cent deal
  amount, negative opening balance) asserts that all three are listed. *Guard*: for each rule, comment out that
  rule's check and confirm its test fails; record. Run; record failing.
- [ ] T019 Implement `Validation/ScenarioValidator.cs` (shared rules) until T018 passes.
- [ ] T020 Write `CommissionEngineTests` (`FR-003`, `FR-004`, `FR-005`): an invalid scenario returns
  `RejectedScenario` and no statements; a valid one returns one `RepStatement` per roster rep in
  roster order whose first line is "Quarterly quota" citing FR-005; every line's RequirementId
  matches `FR-\d{3}`. *Guard*: skip the validation call and confirm the rejected-scenario test
  fails; record. Run; record failing.
- [ ] T021 Implement `CommissionEngine.Calculate` and `Calculation/StatementBuilder.cs` (validation
  first, then statement assembly) until T020 passes.
- [ ] T022 Create `tests/CommissionCalculator.Web.Tests/` (xunit.v3 unit tests of single web
  classes) and write `ScenarioFileReaderTests` (`FR-001`, `FR-004`): a file matching
  contracts/scenario-file.md maps to the expected `ScenarioInput`; `10.005` is read exactly;
  unknown property, malformed JSON and a missing required field each produce a load error naming
  the file, never an exception out of the reader. *Guard*: remove the reader's error capture and
  confirm the malformed-JSON test fails with an escaped exception; record. Run; record failing.
- [ ] T023 Write `ScenarioCatalogTests` in Web.Tests (`FR-001`): files are listed ordered by file
  name; a file that fails to load is listed as a load error and does not hide the others.
  *Guard*: make the catalog stop at the first failing file and confirm the "does not hide the
  others" test fails; record. Run; record failing.
- [ ] T024 Implement `ScenarioCatalog/ScenarioFileReader.cs` and `ScenarioCatalog.cs` (loads every
  `Scenarios/*.json` from a directory set by an options value that defaults to
  `Path.Combine(AppContext.BaseDirectory, "Scenarios")` — the build/publish output, not the content
  root, so a file missing from output is missing at run time; tests can point it at a directory
  they create) until T022–T023 pass.
- [ ] T025 Confirm in the real solution that `Scenarios/*.json` reaches build and publish output
  through the Web SDK's default content items, with **no** project-file entry (research R17: an
  explicit `<Content Include>` fails the build with NETSDK1022) — structural; proven by T035's smoke
  assertion and guard.
- [ ] T026 Checkpoint: PR "Phase 2: Foundational", CI green, maintainer approves squash-merge.

---

## Phase 3: User Story 1 — tiered commission for a scenario (P1) 🎯 MVP — PR 3

**Goal**: pick a scenario and see each rep's tier lines with FR citations.
**Independent test**: `Features/US1_TieredCommission.feature` passes; the page renders the
`tiers` scenario.

- [ ] T027 [P] [US1] In `tests/CommissionCalculator.Specs/`, add step definitions that build
  `ScenarioInput` from Gherkin tables and assert, to the cent, both
  statement lines (description, amount, FR) and statement summary fields (`Attainment`,
  `CreditedBookings`, `ProratedQuota`, `DrawPaid`, `EarnedCommission`, `Payable`,
  `ClosingRecoverableBalance`). Write `Features/US1_TieredCommission.feature` with US1 AS1–AS4
  exact, including AS1's attainment of 80% (`@FR-006 @FR-007 @FR-009 @FR-018`). Run; record
  failing.
- [ ] T028 [P] [US1] Write `TierScheduleTests` (`FR-006`, `FR-007`): credit 80,000/100,000 → one 5%
  line 4,000.00; 160,000 → 5,000.00 / 4,000.00 / 1,200.00; exactly 150,000 → no 12% line;
  exactly 100,000 → no 8% line; 10.10 → 0.51; 150% edge from a rounded quota of 50,549.45 is
  75,824.18. Run; record failing.
- [ ] T029 [US1] Implement `Calculation/TierSchedule.cs` and crediting of single-rep deals in
  `Calculation/QuarterCredit.cs` wired into `StatementBuilder` (credit lines FR-008, tier lines
  FR-006, "Commission before refunds", attainment FR-009) until T027–T028 pass.
- [ ] T030 [P] [US1] Write `Features/UI_Page.feature` in Specs, driven through the web host
  (`@FR-001 @FR-002 @FR-003 @FR-021 @SC-002`): picker form structure per contracts/ui.md;
  `?scenario=tiers` shows one section per rep with `h2`, caption, `th scope=col` Item/Amount/Rule;
  every Rule cell is an FR ID that exists in spec.md (read from the committed spec file); each rep's
  page has a `<title>` naming the selected scenario (WCAG 2.4.2); these scenarios run against a scenario directory the scenario itself creates (a temp folder holding the Appendix A.1 inputs
  as JSON), never the shipped `Scenarios/` folder, which is only added in Phase 9 (T060a); summary `dl` shows quota, prorated quota, credited bookings, attainment (Avery: "80.00%"),
  earned, draws paid, payable and closing balance; unknown id → 404 with the picker; a load error
  is shown — for this one scenario the host's scenario directory is a temp directory the scenario
  creates (one valid file, one malformed), never the shared build output. *Guard*: return 200 for an unknown id and confirm that scenario fails; record. Run;
  record failing. Add `AttainmentFormatTests` in Web.Tests (`FR-002`): 0.8 → "80.00%", 0.12345 →
  "12.35%" (midpoint, half away from zero), 1.6 → "160.00%". *Guard*: format with banker's rounding
  and confirm the midpoint case fails ("12.34%"); record. Add `MoneyFormatTests` in Web.Tests
  (`FR-021`, `FR-018`): 1234.5 → "$1,234.50", 0 → "$0.00", −1600 → "−$1,600.00" (minus sign in
  the text). *Guard*: format with `Math.Abs` and confirm the negative case fails; record. (T032's
  formatting is written against these, not against T057, which arrives in Phase 8.)
- [ ] T031 [P] [US1] Write `StylesheetAccessibilityTests` in Web.Tests (`FR-021`), reading
  `wwwroot/css/site.css`: 1.4.3 text contrast — body, table and link text on their backgrounds
  ≥ 4.5:1; 1.4.11 non-text contrast — focus indicator and `select`/`button` borders ≥ 3:1 against
  adjacent colours; 2.5.8 target size — `select`, `button` and the skip link have a minimum height
  and width of at least 24px; 2.4.11 focus not obscured — no `position: fixed` or `sticky` rules;
  1.4.12 text spacing — no fixed `height` and no `overflow: hidden` on text containers.
  *Guard*: set the text token to a light grey, and separately set the button's min-height to 16px;
  confirm each makes its test fail; record. Run; record failing.
- [ ] T032 [US1] Implement the page (picker, the empty state "No scenarios are installed",
  statements, summary `dl`, breakdown tables, `$#,##0.00` with a minus sign, `site.css` colour
  tokens and visible focus styles) until T030–T031 pass.
- [ ] T033 [US1] Keyboard-only check (FR-021, SC-005; standing rule 3): run the app with its
  scenario directory pointed at a folder holding the Appendix A.1 and A.2 inputs, and drive it with
  key presses only through the in-app browser (evidence: the action log contains no pointer
  events); Tab to the picker, change scenario with arrow keys, submit with Enter, Tab through each
  table; confirm visible focus at every stop. Then resize the viewport to 320 CSS px wide (1.4.10
  reflow; evidence: the viewport width reported by the browser just before the screenshot) and
  confirm no horizontal page scroll other than inside the breakdown tables. Record the log excerpt
  and screenshot in the PR.
- [ ] T034 [US1] Screen-reader check (FR-021; standing rule 3) — **maintainer step**: with
  VoiceOver on (evidence: VoiceOver caption panel visible in a screenshot), navigate by headings
  and tables on `?scenario=tiers` (app run as in T033); confirm each rep's `h2`, the table caption and column headers are
  announced. The agent does not change system accessibility settings; the maintainer records the
  result on the PR.
- [ ] T035 [US1] Add a second CI job, **offline smoke**: `dotnet publish` the web app, run it in
  `mcr.microsoft.com/dotnet/aspnet:10.0.12` with `docker run --network none`, and request `/` (HTTP
  200 with the picker; the seed-table assertion joins it in T060b) from a `curlimages/curl` container started with `--network container:<app>`
  (the runtime image has no HTTP client; research R16); as a negative control the same sidecar's
  request to an external host must fail (SC-004; evidence: `docker inspect` shows
  `NetworkMode: none`, printed immediately before the request). Both jobs write a TRX-shaped result
  (`ci-evidence/*.trx`) naming their check with category `SC-004` (and `FR-020` for the smoke step),
  pass or fail, so the trace sees them as tests. *Guard*: point the sidecar's request at a path
  that returns 404 and confirm the job fails; record.
- [ ] T036 [US1] Checkpoint: PR "Phase 3: US1", CI green, maintainer approves squash-merge. Record
  the offline job's first run in research R16 (Docker on the runner) and confirm the plan's
  Provenance gate; propose to the maintainer a PATCH amendment adding Docker (a CI-only dependency
  of the offline-smoke job) to constitution C7 and its pairwise note.

---

## Phase 4: User Story 2 — booking date decides the quarter (P1) — PR 4

**Goal**: only deals booked in the quarter count; excluded deals are listed with the reason.
**Independent test**: `Features/US2_BookingDate.feature` passes.

- [ ] T037 [P] [US2] Write `Features/US2_BookingDate.feature`: US2 AS1–AS4 with exact dates and
  amounts (`@FR-008`), including the excluded-deal line citing FR-008. Run; record failing.
- [ ] T038 [P] [US2] Write `QuarterCreditTests` (`FR-008`): booked on the first and last day counts;
  booked the day before/after is excluded; close date never changes the result. Add
  `ScenarioValidatorTests` (`FR-004`): refund dated before booking date is rejected. *Guard*:
  switch crediting to close date and confirm AS1 (B-1) and AS2 (B-2) fail; remove the refund-date
  check and confirm its test fails; record. Run; record failing.
- [ ] T039 [P] [US2] Write `Features/US1_ScenarioSwitch.feature` in Specs, driven through the web
  host (`@FR-001`, US1 AS5 — placed here because it needs the second scenario's rules): with
  a scenario directory the scenario itself creates (a temp folder holding the Appendix A.1 and A.2 inputs
  as JSON), never the shipped `Scenarios/` folder, which is only added in Phase 9 (T060a) — both present, selecting `booking-dates` shows only Emery, and selecting `tiers`
  shows only Avery–Devon. Run; record failing.
- [ ] T040 [US2] Implement booking-date crediting and excluded-deal lines until T037–T039 pass.
- [ ] T041 [US2] Checkpoint: PR "Phase 4: US2", CI green, maintainer approves squash-merge.

---

## Phase 5: User Story 3 — prorated quota (P2) — PR 5

**Goal**: mid-quarter starters are measured against a calendar-day-prorated quota.
**Independent test**: `Features/US3_Proration.feature` passes.

- [ ] T042 [P] [US3] Write `Features/US3_Proration.feature`: US3 AS1–AS4 exact, including the
  prorated-quota summary values (`@FR-010 @FR-009 @FR-018`). Run; record failing.
- [ ] T043 [P] [US3] Write `QuotaProrationTests` (`FR-010`, `FR-004`, `FR-011`): 45/90 of
  90,000.00 = 45,000.00; 46/91 of 100,000.00 = 50,549.45; start on or before the first day → no
  proration and no prorated-quota line; start on the last day → 1 day; prorated quota rounding to
  0.00 → rejected; start after quarter end → rejected naming the rep; any deal (counted or not)
  booked before a credited rep's start → rejected naming deal and rep. *Guard*: remove each
  rejection check and confirm its test fails; record. Run; record failing.
- [ ] T044 [US3] Implement `Calculation/QuotaProration.cs` and the FR-011/FR-004 start-date rules
  until T042–T043 pass.
- [ ] T045 [US3] Checkpoint: PR "Phase 5: US3", CI green, maintainer approves squash-merge.

---

## Phase 6: User Story 4 — split deals (P2) — PR 6

**Goal**: split shares by largest remainder, credited toward attainment and commission.
**Independent test**: `Features/US4_Splits.feature` passes.

- [ ] T046 [P] [US4] Write `Features/US4_Splits.feature`: US4 AS1–AS5 exact, including AS2's
  credited bookings of $110,000.00 and attainment of 110% (`@FR-012 @FR-013 @FR-009`). Run; record
  failing.
- [ ] T047 [P] [US4] Write `SplitAllocationTests` (`FR-012`): 60/40 of 50,000.00; 50/50 of 10.01 →
  5.01/5.00; 33.335/33.335/33.33 of 100.00 → 33.34/33.33/33.33; 45/45/10 of 0.06 → 0.03/0.03/0.00;
  45/45/10 of 0.05 → 0.02/0.02/0.01 (US6 AS9's re-split); shares always sum to the amount over a
  fixed table of cases. Add validator tests (`FR-013`): sums of 99.999 and 100.001 rejected; 100
  accepted. *Guard*: replace largest remainder with independent rounding and confirm the 10.01
  case fails; remove the sum check and confirm the FR-013 tests fail; record. Run; record failing.
- [ ] T048 [P] [US4] Write `Features/UI_RejectedScenario.feature` in Specs, driven through the web
  host (`@FR-004`), against a scenario directory the scenario itself creates (a temp folder holding the Appendix A..11 inputs
  as JSON), never the shipped `Scenarios/` folder, which is only added in Phase 9 (T060a): `?scenario=invalid` renders an element with `role="alert"` listing four errors,
  each with its FR ID, and no rep table. *Guard*: stop rendering the alert's error list and confirm
  the scenario fails; record. ("No rep table" needs no separate guard: `RejectedScenario` carries
  no statements, and T020 guards the engine side.) Run;
  record failing.
- [ ] T049 [US4] Implement `Calculation/SplitAllocation.cs`, split crediting in `QuarterCredit`
  (share lines cite FR-012), the FR-013 rule and the rejected-scenario view until T046–T048 pass.
- [ ] T050 [US4] Checkpoint: PR "Phase 6: US4", CI green, maintainer approves squash-merge.

---

## Phase 7: User Story 5 — monthly draw and recovery (P2) — PR 7

**Goal**: draws per month (start month prorated), recovery, payable, carried balance.
**Independent test**: `Features/US5_Draw.feature` passes.

- [ ] T051 [P] [US5] Write `Features/US5_Draw.feature`: US5 AS1–AS6 exact, including AS6's total
  draw of $9,548.39 (`@FR-014 @FR-015`). Run; record failing.
- [ ] T052 [P] [US5] Write `DrawScheduleTests` (`FR-014`): full quarter = 3 × 4,000.00; start
  2026-02-15 → 0.00 / 2,000.00 / 4,000.00; start 2026-01-20 → 1,548.39; start on the 1st of the
  second month → 0.00 / 4,000.00 / 4,000.00. Write `DrawRecoveryTests` (`FR-015`): earned ≥
  recoverable total; earned < total; earned = 0; earned negative (recovered 0.00, payable 0.00,
  closing = total + |earned|); opening balance included; `PayableIsNeverNegative` over those cases.
  *Guard*: remove the negative-earned branch — expected result: recovered becomes −1,600.00 in the
  negative case, so that test fails; record. There is no separate payable clamp to remove (payable
  is `earned − min(earned, total)` ≥ 0 when earned ≥ 0); `PayableIsNeverNegative` is therefore a
  property check, not a failure-path guard — recorded here per standing rule 2. Run; record
  failing.
- [ ] T053 [US5] Implement `Calculation/DrawSchedule.cs` and `Calculation/DrawRecovery.cs` and
  their statement lines until T051–T052 pass.
- [ ] T054 [US5] Checkpoint: PR "Phase 7: US5", CI green, maintainer approves squash-merge.

---

## Phase 8: User Story 6 — clawback on refunds (P3) — PR 8

**Goal**: clawbacks per refund, in the refund's quarter, sized by recomputing the booking quarter.
**Independent test**: `Features/US6_Clawback.feature` passes.

- [ ] T055 [P] [US6] Write `Features/US6_Clawback.feature`: US6 AS1–AS9 exact, including the
  re-split lines (FR-017) — AS7's $4,950.10 / $4,950.09 and AS9's $0.01 — (`@FR-016 @FR-017`), and
  the Q2 scenarios with booking-quarter data. Run; record failing.
- [ ] T056 [P] [US6] Write `ClawbackCalculatorTests` (`FR-016`, `FR-017`): full, partial and
  repeated refunds; refunds across deals ordered by date then deal then refund position; refund
  after quarter end ignored; negative clawback from a re-split; split refund re-split
  (4,950.10 / 4,950.09). Add validator tests (`FR-004`): refunds totalling more than the deal;
  missing/incomplete booking-quarter data; booking quarter overlapping or malformed; rep start-date
  mismatch; a booking-quarter deal whose booking date is outside that quarter's dates → rejected;
  partner listed with start date accepted; deal in both lists differing → rejected; a
  refund of 0.00 or less → rejected; a booking-quarter deal booked before the start date of a
  credited roster rep or partner → rejected naming the deal and the rep (FR-011). Add a statement test: a refund on a split deal adds a re-split
  line citing FR-017 before its clawback line; a refund on a single-rep deal adds none.
  *Guard*: (a) size each refund against the untouched quarter and confirm AS6 fails; (b) remove
  the refund-date filter and confirm AS3 and AS5 (Q1) fail; (c) floor clawbacks at zero and
  confirm AS9 fails; (d) remove each new validation check (including refund ≤ 0 and the
  booking-quarter start-date check) and confirm its test fails; record all.
  Run; record failing.
- [ ] T057 [P] [US6] Write `Features/UI_NegativeAmounts.feature` in Specs, driven through the web
  host (`@FR-021 @FR-015`): `?scenario=refunds-q2` renders Sage's earned commission as "−$1,600.00"
  with a minus sign in the text (not colour alone), against a scenario directory the scenario itself creates (a temp folder holding the Appendix A..10 inputs
  as JSON), never the shipped `Scenarios/` folder, which is only added in Phase 9 (T060a). Its
  first red is recorded with its actual cause (no clawback line yet, so earned is not negative);
  the formatting itself was test-driven in T030's `MoneyFormatTests`. *Guard*: format with `Math.Abs` and confirm the test fails; record.
  Run; record failing.
- [ ] T058 [US6] Implement `Calculation/ClawbackCalculator.cs`, refund-aware `QuarterCredit`,
  booking-quarter validation and clawback lines until T055–T057 pass.
- [ ] T059 [US6] Checkpoint: PR "Phase 8: US6", CI green, maintainer approves squash-merge.

---

## Phase 9: Polish & cross-cutting — PR 9

- [ ] T060 [P] Write `Features/SeededScenarios.feature` (`@SC-001 @SC-003`): for every file in
  `Scenarios/`, load it through the real reader and assert every rep's statement equals Appendix A
  line for line (description, amount to the cent, FR), and `invalid.json` lists exactly the four
  Appendix A.11 errors. SC-003 check: the brief's rules map to FRs as rule 1 → FR-005, rule 2 →
  FR-006, rule 3 → FR-010, rule 4 → FR-012, rule 5 → FR-014/FR-015, rule 6 → FR-016, rule 7 →
  FR-008 (an excluded-by-booking-date line), rule 8 → FR-019 (satisfied by scope, as in T061); assert
  that for rules 1–7 at least one seeded statement contains a line citing the mapped FR. *Guard*:
  remove `booking-dates.json` from the check's input and confirm rule 7 fails; record. The feature
  lists the eleven expected files by name, so before T060a it fails on every one ("scenario file
  not found"); run it and record that red. Any later failure is a spec question, not a test edit.
- [ ] T060a Add the eleven seed files `src/CommissionCalculator.Web/Scenarios/{tiers,
  booking-dates, proration, proration-q2, splits, split-rounding, draw, refunds, refund-splits,
  refunds-q2, invalid}.json`, transcribing Appendix A.1–A.11 inputs, until T060 passes. A mismatch
  is fixed in the seed file, never in Appendix A or the test.
- [ ] T060b Extend the CI smoke step (T010) and the offline-smoke job (T035): request
  `/?scenario=tiers` and require a `<table>` containing "Avery" — proving the running app loaded the
  seed files from its output. *Guard*: on a throwaway branch add `<Content Update="Scenarios/*.json"
  CopyToOutputDirectory="Never" CopyToPublishDirectory="Never" />` and confirm both assertions fail
  (research R17 observed that this removes the files from output); record.
- [ ] T061 Traceability generator, test first: add `TraceabilityTests` to Tools.Tests
  (`[Trait("Principle", "III")]`; fixture spec with FR-001..FR-003 and SC-001, fixture TRX, fixture
  assembly metadata) — an ID with no test and an ID with no member both appear under "Gaps"; an ID
  declared in the scope-only list appears under "Satisfied by scope" with its reason and not under
  Gaps; an SC in the evidence-only list with tests and no member is not a gap, and with no tests it
  is; an ID in the manual-evidence list appears under "Manual evidence" with its task reference and
  not under Gaps; tests carrying a `Principle` trait appear under "Tooling tests" and not against any FR; a
  CI evidence TRX (`ci-evidence/*.trx`) contributes its categories like any other TRX.
  *Guard*: disable gap detection and confirm the gaps test fails; record. Run; record failing. Then add the
  `trace` command to `tools/CommissionCalculator.Tools` (FR/SC IDs from spec.md; test → IDs from
  TRX categories/traits; member → IDs from `[Implements]` via reflection over the built engine and
  web assemblies; scope-only list: FR-019 — "USD only; tax, currency conversion and
  multi-year are out of scope, so no member implements them"; evidence-only list: SC-001 — seed
  files and Appendix A, SC-003 — the seeded scenarios, SC-004 — the smoke jobs, each verified by
  tests but implemented by no single member, and SC-002 — every line cites an FR, verified by T030 and T060; manual-evidence list:
  SC-005 and FR-021's
  screen-reader clause — T033/T034, not visible to the trace) writing
  `specs/001-commission-calculator/traceability.md`. Add a final CI job, `traceability`, that
  depends on the build/test, smoke and offline-smoke jobs, downloads all their TRX artifacts (suite
  and `ci-evidence/*.trx`), runs `trace` over them and uploads the result.
- [ ] T062 Add `[Implements]` to every engine and web member that implements an FR; run `trace`
  over every TRX (as the CI job does); every FR has at least one member and one test except the
  scope-only FR-019; every SC except the manual-evidence SC-005 has at least one test; the
  manual-evidence items are listed with the PR links recorded in T033/T034/T064.
- [ ] T063 Re-verify every failure-path guard added in T007, T008, T009, T010, T018, T020, T022, T023,
  T030 (attainment and money format), T060, T060b,
  T030, T031, T035, T038, T043, T047, T048, T052, T056, T057, T061 still fails with its guarded
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
