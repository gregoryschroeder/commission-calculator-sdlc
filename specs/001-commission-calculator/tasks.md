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
   and has no dedicated test.
2. **Failure-path guards are watched failing.** A test whose job is to catch something going wrong
   (a rejection, a clawback, a negative-earnings path, a CI gate) names how its guarded behaviour
   will be removed to show the test failing, and the observed result is written into the task when
   it is ticked. Polish re-checks every such guard.
3. **Condition-dependent checks evidence the condition.** A manual check that only means something
   under a condition (fresh clone, keyboard only, screen reader on) records evidence that the
   condition held just before the observation.
4. **Every phase is one pull request.** Each phase ends with a checkpoint task: open the phase's
   PR, CI green, maintainer approves the squash-merge. No PR spans two phases; phases run in order.
5. **Runtime dependencies are tasks.** This feature adds one runtime dependency: the seeded
   scenario JSON files, which must be copied to the web app's output and exercised by CI's smoke
   run (T008, T043). It adds no environment variables, secrets, services or configuration.
6. **Every test names what it verifies**: `[Trait("Requirement", "FR-0xx")]` on unit/web tests,
   `@FR-0xx` / `@SC-00x` tags on Gherkin scenarios. Every acceptance example is tested with its
   exact inputs and asserted to the cent. A test is never edited to make it pass; a test that looks
   wrong is a spec question for the maintainer.

---

## Phase 1: Setup (shared infrastructure) — PR 1

**Purpose**: solution skeleton, build rules and the CI review gate.

- [ ] T001 Create `global.json` (SDK 10.0.400, `rollForward: latestFeature`, test runner
  `Microsoft.Testing.Platform`), `Directory.Build.props` (net10.0, nullable, implicit usings,
  `TreatWarningsAsErrors`, `AnalysisLevel` latest-recommended, `EnforceCodeStyleInBuild`),
  `Directory.Packages.props` (central versions per plan.md), `.gitignore`, and
  `CommissionCalculator.slnx` — structural, no test.
- [ ] T002 Create `src/CommissionCalculator.Engine/CommissionCalculator.Engine.csproj` (no package
  references) and `src/CommissionCalculator.Web/CommissionCalculator.Web.csproj` (Razor Pages,
  references Engine only; `public partial class Program` for tests) — structural, no test.
- [ ] T003 Create `tests/CommissionCalculator.Web.Tests/` (xunit.v3, Mvc.Testing, AngleSharp,
  TrxReport, CodeCoverage) and write the first test **before** the page exists:
  `HomePage_ReturnsOk_WithLangAndMain` (`[Trait("Requirement","FR-020")]`) — GET `/` returns 200,
  `<html lang="en">`, one `<main>`. Run it; record it failing.
- [ ] T004 Implement the minimal `Program.cs` and `Pages/Index.cshtml` (layout with `lang`, skip
  link, `<main>`, local `wwwroot/css/site.css`) until T003 passes.
- [ ] T005 Write `.github/workflows/ci.yml`: checkout@v7, setup-dotnet@v6 from `global.json`,
  `dotnet build -warnaserror`, `dotnet test --fail-skips on --report-trx --coverage
  --coverage-output-format cobertura`, upload TRX and coverage as artifacts, job summary.
- [ ] T006 Add `tools/ci/coverage-gate.sh` (reads the engine assembly's line rate from cobertura,
  prints it to the job summary, exits non-zero below 80%) and call it from CI. Until the engine has
  code the gate reports "no engine lines yet" and passes only in that exact case.
  *Guard*: run it once locally with the threshold set to 101% against a real report and record it
  failing; restore 80%.
- [ ] T007 Add `tools/ci/isolated-tests.sh`: reads the suite TRX, and for each test in the Specs and
  Web test projects runs `dotnet test --project <proj> --filter-method <className.name>` alone;
  fails if any fails. Call it from CI. *Guard*: on a local throwaway branch add a Web test that
  passes only when another test ran first (static flag), confirm the suite passes and the script
  fails; delete the throwaway test; record the result.
- [ ] T008 CI smoke step: start `dotnet run --project src/CommissionCalculator.Web` in the
  background from the fresh checkout, poll `/` until HTTP 200 (fail after 60 s), stop it.
- [ ] T009 *Guard (skip gate)*: on a local throwaway change add `[Fact(Skip="probe")]`, run the CI
  test command, confirm a non-zero exit ("Failed!"); remove it; record the result. (R5 observed
  this in the spike; this re-observes it in the real solution.)
- [ ] T010 `README.md`: what it is, `dotnet run --project src/CommissionCalculator.Web` (or
  `dotnet run` inside that folder), test commands, where the spec lives.
- [ ] T011 Checkpoint: open PR "Phase 1: Setup", CI green, maintainer approves squash-merge.

---

## Phase 2: Foundational (blocking prerequisites) — PR 2

**Purpose**: engine contract types, money rules, the engine entry point and the scenario-file
reader that every story uses.

- [ ] T012 Create the engine's public input/result records and `ImplementsAttribute` exactly as in
  contracts/engine-api.md (`Inputs/`, `Results/`) — structural, no test.
- [ ] T013 Create `tests/CommissionCalculator.Engine.Tests/` and write `MoneyTests`
  (`FR-018`, `FR-004`): half away from zero (0.505→0.51, −0.505→−0.51, 1548.387…→1548.39,
  50549.4505…→50549.45); whole-cent check true for 10.10, false for 10.005. Run; record failing.
- [ ] T014 Implement `Calculation/Money.cs` until T013 passes.
- [ ] T015 Write `QuarterTests` (`FR-004`, `FR-014`): a 2026-01-01..03-31 quarter has 90 days and
  months Jan/Feb/Mar; 2026-04-01..06-30 has 91; a quarter not starting on the 1st or not spanning
  three whole months is invalid. Run; record failing.
- [ ] T016 Implement quarter day/month helpers (internal, in `Calculation/`) until T015 passes.
- [ ] T017 Write `ScenarioValidatorTests` for the shared rules (`FR-004`): quota ≤ 0, deal amount
  ≤ 0, sub-cent money, negative opening balance, empty roster, bad quarter shape, deal credited to
  a rep not on the roster, same rep twice on a deal, split % ≤ 0 or > 100 — each yields its error
  and the scenario is rejected with *all* errors listed. *Guard*: for each rule, comment out that
  rule's check and confirm its test fails; record the result. Run; record failing.
- [ ] T018 Implement `Validation/ScenarioValidator.cs` (shared rules) until T017 passes.
- [ ] T019 Write `CommissionEngineTests` (`FR-003`, `FR-004`, `FR-005`): an invalid scenario returns
  `RejectedScenario` and no statements; a valid one returns one `RepStatement` per roster rep in
  roster order whose first line is "Quarterly quota" citing FR-005; every line's RequirementId
  matches `FR-\d{3}`. Run; record failing.
- [ ] T020 Implement `CommissionEngine.Calculate` and `Calculation/StatementBuilder.cs` (validation
  first, then statement assembly) until T019 passes.
- [ ] T021 Write `ScenarioFileReaderTests` in Web.Tests (`FR-001`, `FR-004`): a file matching
  contracts/scenario-file.md maps to the expected `ScenarioInput`; `10.005` is read exactly;
  unknown property, malformed JSON and a missing required field each produce a load error naming
  the file, never an exception out of the reader. Run; record failing.
- [ ] T022 Implement `ScenarioCatalog/ScenarioFileReader.cs` and `ScenarioCatalog.cs` (loads every
  `Scenarios/*.json` at startup, ordered by file name; load errors kept for display) until T021
  passes. Add `<Content Include="Scenarios/*.json" CopyToOutputDirectory="PreserveNewest" />`.
- [ ] T023 Checkpoint: PR "Phase 2: Foundational", CI green, maintainer approves squash-merge.

---

## Phase 3: User Story 1 — tiered commission for a scenario (P1) 🎯 MVP — PR 3

**Goal**: pick a scenario and see each rep's tier lines with FR citations.
**Independent test**: `Features/US1_TieredCommission.feature` passes; the page renders the
`tiers` scenario.

- [ ] T024 [P] [US1] Create `tests/CommissionCalculator.Specs/` (Reqnroll.xUnit.v3) with step
  definitions that build `ScenarioInput` from Gherkin tables and assert statement lines by
  description and amount to the cent. Write `Features/US1_TieredCommission.feature` with US1 AS1–AS5,
  exact values (`@FR-006 @FR-007 @FR-018 @FR-001`). Run; record failing.
- [ ] T025 [P] [US1] Write `TierScheduleTests` (`FR-006`, `FR-007`): credit 80,000/100,000 → one 5%
  line 4,000.00; 160,000 → 5,000.00 / 4,000.00 / 1,200.00; exactly 150,000 → no 12% line;
  exactly 100,000 → no 8% line; 10.10 → 0.51; 150% edge from a rounded quota of 50,549.45 is
  75,824.18. Run; record failing.
- [ ] T026 [US1] Implement `Calculation/TierSchedule.cs` and full-credit crediting of a rep's own
  deals booked in the quarter (`Calculation/QuarterCredit.cs`, single-rep deals only) wired into
  `StatementBuilder` (credit lines FR-008, tier lines FR-006, "Commission before refunds") until
  T024–T025 pass.
- [ ] T027 [P] [US1] Write `IndexPageTests` (`FR-001`, `FR-002`, `FR-003`, `FR-021`, `SC-002`,
  `SC-005`): picker form structure per contracts/ui.md; `?scenario=tiers` shows one section per
  rep with `h2`, caption, `th scope=col` Item/Amount/Rule; every Rule cell is an FR ID that exists
  in spec.md (read from the spec file); unknown id → 404 with the picker; a load error is shown.
  Run; record failing.
- [ ] T028 [US1] Add `Scenarios/tiers.json` (Appendix A.1 inputs) and implement the page
  (picker, statements, summary `dl`, breakdown tables, `$#,##0.00` with a minus sign) until T027
  passes.
- [ ] T029 [US1] Checkpoint: PR "Phase 3: US1", CI green, maintainer approves squash-merge.

---

## Phase 4: User Story 2 — booking date decides the quarter (P1) — PR 4

**Goal**: only deals booked in the quarter count; excluded deals are listed with the reason.
**Independent test**: `Features/US2_BookingDate.feature` passes.

- [ ] T030 [P] [US2] Write `Features/US2_BookingDate.feature`: US2 AS1–AS4 with exact dates and
  amounts (`@FR-008`), including the excluded-deal line citing FR-008. Run; record failing.
- [ ] T031 [P] [US2] Write `QuarterCreditTests` (`FR-008`): booked on first and last day counts;
  booked the day before/after is excluded; close date never changes the result. Write
  `ScenarioValidatorTests` additions (`FR-004`): refund dated before booking date is rejected.
  *Guard*: remove the booking-date comparison (use close date) and confirm AS1 and AS4 fail;
  record. Run; record failing.
- [ ] T032 [US2] Implement booking-date crediting and excluded-deal lines until T030–T031 pass;
  add `Scenarios/booking-dates.json` (Appendix A.2).
- [ ] T033 [US2] Checkpoint: PR "Phase 4: US2", CI green, maintainer approves squash-merge.

---

## Phase 5: User Story 3 — prorated quota (P2) — PR 5

**Goal**: mid-quarter starters are measured against a calendar-day-prorated quota.
**Independent test**: `Features/US3_Proration.feature` passes.

- [ ] T034 [P] [US3] Write `Features/US3_Proration.feature`: US3 AS1–AS4 exact (`@FR-010
  @FR-009 @FR-018`). Run; record failing.
- [ ] T035 [P] [US3] Write `QuotaProrationTests` (`FR-010`, `FR-004`, `FR-011`): 45/90 of
  90,000.00 = 45,000.00; 46/91 of 100,000.00 = 50,549.45; start on or before the first day → no
  proration and no prorated-quota line; start on the last day → 1 day; prorated quota rounding to
  0.00 → rejected; start after quarter end → rejected naming the rep; any deal (counted or not)
  booked before a credited rep's start → rejected naming deal and rep. *Guard*: remove each
  rejection check and confirm its test fails; record. Run; record failing.
- [ ] T036 [US3] Implement `Calculation/QuotaProration.cs` and the FR-011/FR-004 start-date rules
  until T034–T035 pass; add `Scenarios/proration.json` and `Scenarios/proration-q2.json`
  (Appendix A.3, A.4).
- [ ] T037 [US3] Checkpoint: PR "Phase 5: US3", CI green, maintainer approves squash-merge.

---

## Phase 6: User Story 4 — split deals (P2) — PR 6

**Goal**: split shares by largest remainder, credited toward attainment and commission.
**Independent test**: `Features/US4_Splits.feature` passes.

- [ ] T038 [P] [US4] Write `Features/US4_Splits.feature`: US4 AS1–AS5 exact (`@FR-012 @FR-013`).
  Run; record failing.
- [ ] T039 [P] [US4] Write `SplitAllocationTests` (`FR-012`): 60/40 of 50,000.00; 50/50 of 10.01 →
  5.01/5.00; 33.335/33.335/33.33 of 100.00 → 33.34/33.33/33.33; 45/45/10 of 0.06 → 0.03/0.03/0.00;
  shares always sum to the amount (property check over a fixed table of cases). Write validator
  additions (`FR-013`): sums of 99.999 and 100.001 rejected; 100 accepted. *Guard*: replace
  largest remainder with independent rounding and confirm the 10.01 case fails; remove the sum
  check and confirm the FR-013 tests fail; record. Run; record failing.
- [ ] T040 [US4] Implement `Calculation/SplitAllocation.cs`, split crediting in `QuarterCredit`
  (share lines cite FR-012) and the FR-013 rule until T038–T039 pass; add `Scenarios/splits.json`,
  `Scenarios/split-rounding.json` and `Scenarios/invalid.json` (Appendix A.5, A.6, A.11).
- [ ] T041 [US4] Checkpoint: PR "Phase 6: US4", CI green, maintainer approves squash-merge.

---

## Phase 7: User Story 5 — monthly draw and recovery (P2) — PR 7

**Goal**: draws per month (start month prorated), recovery, payable, carried balance.
**Independent test**: `Features/US5_Draw.feature` passes.

- [ ] T042 [P] [US5] Write `Features/US5_Draw.feature`: US5 AS1–AS6 exact (`@FR-014 @FR-015`).
  Run; record failing.
- [ ] T043 [P] [US5] Write `DrawScheduleTests` (`FR-014`): full quarter = 3 × 4,000.00; start
  2026-02-15 → 0.00 / 2,000.00 / 4,000.00; start 2026-01-20 → 1,548.39; start on the 1st of the
  second month → 0.00 / 4,000.00 / 4,000.00. Write `DrawRecoveryTests` (`FR-015`): earned ≥
  recoverable total; earned < total; earned = 0; earned negative (recovered 0, payable 0, closing
  = total + |earned|); opening balance included. *Guard*: remove the negative-earned branch and
  confirm its test fails; clamp payable removal → confirm the payable-never-negative test fails;
  record. Run; record failing.
- [ ] T044 [US5] Implement `Calculation/DrawSchedule.cs` and `Calculation/DrawRecovery.cs` and
  their statement lines until T042–T043 pass; add `Scenarios/draw.json` (Appendix A.7).
- [ ] T045 [US5] Checkpoint: PR "Phase 7: US5", CI green, maintainer approves squash-merge.

---

## Phase 8: User Story 6 — clawback on refunds (P3) — PR 8

**Goal**: clawbacks per refund, in the refund's quarter, sized by recomputing the booking quarter.
**Independent test**: `Features/US6_Clawback.feature` passes.

- [ ] T046 [P] [US6] Write `Features/US6_Clawback.feature`: US6 AS1–AS9 exact (`@FR-016
  @FR-017`), including the Q2 scenarios with booking-quarter data. Run; record failing.
- [ ] T047 [P] [US6] Write `ClawbackCalculatorTests` (`FR-016`, `FR-017`): full, partial and
  repeated refunds; refunds across deals ordered by date then deal then refund position; refund
  after quarter end ignored; negative clawback from a re-split; split refund re-split
  (4,950.10 / 4,950.09). Write validator additions (`FR-004`): refunds totalling more than the
  deal; missing/incomplete booking-quarter data; booking quarter overlapping or malformed; rep
  start-date mismatch; partner listed with start date accepted; deal in both lists differing →
  rejected. *Guard*: (a) size each refund against the untouched quarter and confirm the
  two-refund case (US6 AS6) fails; (b) remove the refund-date filter and confirm AS4 fails; (c)
  floor clawbacks at zero and confirm AS9 fails; (d) remove each new validation check and confirm
  its test fails; record all. Run; record failing.
- [ ] T048 [US6] Implement `Calculation/ClawbackCalculator.cs`, refund-aware `QuarterCredit`,
  booking-quarter validation and clawback lines until T046–T047 pass; add
  `Scenarios/refunds.json`, `Scenarios/refund-splits.json`, `Scenarios/refunds-q2.json`
  (Appendix A.8–A.10).
- [ ] T049 [US6] Checkpoint: PR "Phase 8: US6", CI green, maintainer approves squash-merge.

---

## Phase 9: Polish & cross-cutting — PR 9

- [ ] T050 [P] Write `Features/SeededScenarios.feature` (`@SC-001 @SC-003`): for every file in
  `Scenarios/`, load it through the real reader and assert every rep's statement equals Appendix A
  line for line (description, amount to the cent, FR), and `invalid.json` lists exactly the four
  Appendix A.11 errors. Also asserts each of the brief's eight rules is exercised by at least one
  seeded scenario (SC-003). Run; any failure is a spec question, not a test edit.
- [ ] T051 [P] Write `tools/Traceability.cs` (R8): FR/SC IDs from spec.md; test → IDs from TRX
  categories/traits; member → IDs from `[Implements]` via reflection over the built engine and web
  assemblies; writes `specs/001-commission-calculator/traceability.md` with an FR/SC → member →
  tests table and a gaps section. Add a CI step that runs it and uploads the file. Test first:
  a small fixture TRX + fixture spec in `tools/tests/` with one ID lacking a test and one lacking a
  member; confirm both appear under gaps; record.
- [ ] T052 Add `[Implements]` to every engine member that implements an FR, and verify the
  generated report lists no FR without an implementing member other than FR-019/FR-020/FR-021
  (satisfied by scope, the app host and the page respectively — each mapped to its web member).
- [ ] T053 Re-verify every failure-path guard added in T006, T007, T009, T017, T031, T035, T039,
  T043, T047 still fails with its guarded behaviour removed; record each result here.
- [ ] T054 Quickstart validation (condition-dependent, standing rule 3): step 1 from a fresh
  clone (evidence: `git status --ignored` clean before running); steps 5–6 keyboard-only and
  VoiceOver (evidence recorded as quickstart.md describes). Record results in the PR.
- [ ] T055 Checkpoint: PR "Phase 9: Polish", CI green, maintainer approves squash-merge.

---

## Dependencies & execution order

- Phases run strictly in order 1 → 9; each starts only after the previous phase's PR is merged
  (standing rule 4). Stories are sequential for one reviewer even where independent.
- Within a phase: test tasks first (seen failing), then implementation; `[P]` test tasks can be
  written together.
- US1 (MVP) delivers a usable app: pick `tiers`, see tier lines with FR citations.
- US2–US6 each add their rules and seed files; Polish adds the full-statement check (SC-001),
  which can only pass once every rule exists.

## Parallel examples

- Phase 3: T024, T025 and T027 are different files — write them together, run, see all fail.
- Phase 8: T046 and T047 together; T048 afterwards.

## Implementation strategy

MVP first (Phases 1–3), then one story per PR in priority order, each independently testable
through its feature file. Each PR description lists the tests seen failing and the guards'
observed results.
