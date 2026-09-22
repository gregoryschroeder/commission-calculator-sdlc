# Implementation Plan: Quarterly Sales Commission Calculator

**Branch**: `001-commission-calculator` | **Date**: 2026-09-21 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-commission-calculator/spec.md`

## Summary

A local, read-only web app: pick a seeded scenario, see each rep's quarterly commission statement
with every amount on its own line citing the FR that produced it. All payout logic lives in a
pure .NET 10 engine library (exact `decimal` money, `DateOnly` dates, no I/O); a Razor Pages app
loads seeded scenarios from local JSON files and renders the engine's statements without
recomputing anything. Tests: xUnit v3 unit tests for the engine; Reqnroll Gherkin features
mirroring every acceptance scenario, to the cent; host-level scenarios asserting the rendered structure and
FR citations (web-host tests are Gherkin too, in the Specs project). CI (GitHub Actions) is the
review gate. Evidence for each choice is in
[research.md](research.md).

## Technical Context

**Language/Version**: C# on .NET 10 (SDK 10.0.400, `global.json`) — research R1

**Primary Dependencies**: ASP.NET Core Razor Pages (shared framework, R2). Test-only: xunit.v3
4.0.1, Reqnroll.xUnit.v3 3.3.4, Microsoft.AspNetCore.Mvc.Testing 10.0.12, AngleSharp 1.8.2,
Microsoft.Testing.Extensions.CodeCoverage 18.11.2, Microsoft.Testing.Extensions.TrxReport 2.4.0
(R3–R9). Engine: no package dependencies.

**Storage**: seeded scenario JSON files in the web project, read at startup (R13). No database.

**Testing**: `dotnet test` on Microsoft.Testing.Platform with `--fail-skips on` (R3, R5)

**Target Platform**: developer machine (macOS/Windows/Linux) with the .NET 10 SDK; CI on
`ubuntu-latest`

**Project Type**: web application with a separate domain library

**Performance Goals**: none specified; a statement is computed in memory from a handful of deals.
No number is stated, so none is invented.

**Constraints**: local only, no external services, no auth, no database (C2); exact to the cent
(Principle II); warnings are errors (Principle V); WCAG 2.2 AA (Principle VI)

**Scale/Scope**: eleven seeded scenarios (spec Appendix A), each with one to five reps

## Constitution Check

*GATE: checked before Phase 0 and re-checked after Phase 1 design. Result: PASS.*

| Principle | Status | How the plan complies |
|---|---|---|
| I. Payout ambiguity is the maintainer's decision | PASS | All 21 payout questions answered in spec Clarifications. Planning surfaced no new payout question; the one gap found (SC-001's scope, below) is about which amounts the spec states, and goes to the maintainer at this gate rather than being decided here. |
| II. Test-first, exact to the cent | PASS | Every acceptance scenario becomes a Gherkin scenario with its exact values (Specs project); unit tests precede each engine component; failure-path tests (validation, clawback, recovery) watched failing; `--fail-skips on`; coverage reported with an 80% floor on the engine; Reqnroll maintenance verified (R4). |
| III. Spec fidelity and traceability | PASS | Tests carry FR/SC traits/tags; engine members carry `[Implements]`; the tools project's `trace` command generates traceability.md from TRX + reflection + spec.md, listing gaps; every breakdown line carries its FR. |
| IV. Evidence before assertion | PASS | research.md tags every decision; two recalled assumptions were corrected by spikes (R3 VSTest packages, R12 `dotnet run` at root) and the corrections are kept. |
| V. Clean architecture, warnings-as-errors | PASS | Engine has no dependency on web, file system or clock; web depends on engine only; money is `decimal`; `TreatWarningsAsErrors` in `Directory.Build.props`, identical in CI. |
| VI. Accessible by default | PASS | Server-rendered semantic HTML per contracts/ui.md; structure asserted by host-level Gherkin scenarios (T030); stylesheet checks by unit test for WCAG 2.2 1.4.3, 1.4.11, 2.5.8, 2.4.11 and 1.4.12 (tasks T031); keyboard-only operation and 1.4.10 reflow at 320 px driven through the browser (T033); VoiceOver by the maintainer (T034). Not claimed: a full WCAG audit — no automated browser scan (axe) runs, so criteria outside the list above are covered only by the semantic-HTML structure asserted in T030, and are not asserted to pass. |
| VII. Runtime environment is part of the feature | PASS | New runtime dependency: the seeded JSON files, copied to build and publish output by the Web SDK's default content items with no project-file entry (research R17), and exercised by CI's smoke runs. No environment variables, secrets, ports beyond the ASP.NET default, or services. |

### Standing Gates *(agentic-sdlc kit)*

| Gate | Status | Notes |
|---|---|---|
| Provenance | PASS | Principle IV. Every research.md decision says how it was established and when. Three claims remain **assumed**, none load-bearing for correctness: R14 (the CI runner resolves SDK 10.0.4xx via setup-dotnet), R15 (the per-test isolation loop's CI time) and R16 (Docker is available on `ubuntu-latest`); the first CI runs prove or disprove each (tasks T012 for R14–R15, T036 for R16). |
| Degraded window | N/A | Principle VII. Nothing is deployed or scaled; the app runs on the developer's machine for as long as they run it. No cold start, pause or reclaim is accepted. |

## Project Structure

### Documentation (this feature)

```text
specs/001-commission-calculator/
├── spec.md
├── plan.md               # this file
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── engine-api.md
│   ├── scenario-file.md
│   └── ui.md
├── checklists/requirements.md
├── tasks.md              # /speckit-tasks
└── traceability.md       # generated by the tools project's `trace` command (traceability step, after converge)
```

### Source Code (repository root)

```text
CommissionCalculator.slnx
global.json                      # SDK pin + Microsoft.Testing.Platform opt-in (R3)
Directory.Build.props            # net10.0, nullable, TreatWarningsAsErrors, analyzers
Directory.Packages.props         # central package versions
README.md

src/
├── CommissionCalculator.Engine/          # pure domain library, no package references
│   ├── Inputs/                           # ScenarioInput, RepInput, DealInput, ... (contract)
│   ├── Results/                          # ScenarioResult, RepStatement, BreakdownLine, ...
│   ├── Validation/ScenarioValidator.cs   # FR-004, FR-011, FR-013
│   ├── Calculation/
│   │   ├── Money.cs                      # rounding, whole-cent check (FR-018)
│   │   ├── QuotaProration.cs             # FR-010
│   │   ├── SplitAllocation.cs            # FR-012 largest remainder
│   │   ├── QuarterCredit.cs              # FR-008, FR-012, refunds applied
│   │   ├── TierSchedule.cs               # FR-006, FR-007
│   │   ├── ClawbackCalculator.cs         # FR-016, FR-017
│   │   ├── DrawSchedule.cs               # FR-014
│   │   ├── DrawRecovery.cs               # FR-015
│   │   └── StatementBuilder.cs           # assembles lines (FR-003)
│   ├── CommissionEngine.cs               # public entry point
│   └── ImplementsAttribute.cs
└── CommissionCalculator.Web/             # Razor Pages; the only runnable project
    ├── Program.cs
    ├── Pages/Index.cshtml(.cs)
    ├── Scenarios/*.json                  # seeded scenarios (contracts/scenario-file.md)
    ├── Catalog/                          # ScenarioFileReader, ScenarioCatalog: JSON → ScenarioInput, load errors
    └── wwwroot/css/site.css

tests/
├── CommissionCalculator.Engine.Tests/    # xUnit v3 unit tests, [Trait("Requirement", ...)]
├── CommissionCalculator.Specs/           # Reqnroll: one .feature per user story + seeded scenarios
├── CommissionCalculator.Web.Tests/       # xUnit unit tests of single web classes (reader, catalog, formatters, stylesheet)
└── CommissionCalculator.Tools.Tests/     # tests for the CI helper and traceability commands

tools/CommissionCalculator.Tools/         # console app: coverage-gate, list-tests, trace (R8)
.github/workflows/ci.yml
```

**Structure Decision**: two production projects so the dependency rule is enforced by project
references (web → engine; engine → nothing). The scenario JSON reader sits in the web project
because it is I/O at the outer boundary; the engine receives already-typed input and still
validates every business rule itself. Gherkin step definitions build `ScenarioInput` from tables
and call `CommissionEngine.Calculate`; the seeded-scenarios feature additionally goes through the
real JSON reader. Tests are split by kind so CI can run the integration projects test-by-test
(R15).

## Decisions taken by the maintainer at this gate (2026-09-21)

1. **SC-001 as written cannot be satisfied.** It requires every line of every seeded rep's
   statement to match a value stated in the spec, but acceptance examples state only the amounts
   they are about (e.g. US1's 80% rep has a draw, recovery and closing balance the spec never
   states). Options: (A) add to spec.md an appendix of full expected statements for each seeded
   scenario — computed by hand from the FRs, independently of the engine, and approved by you —
   so every seeded amount is a stated requirement; or (B) narrow SC-001 to "every amount the spec
   states for a seeded rep matches, to the cent", leaving the other lines covered by FR-level
   tests. Recommendation: (A) — the payouts shown in the app should be ones you approved.
   **Maintainer: "1: A".** The appendix is added to spec.md before tasks and approved with the
   planning pull request.
2. **How the brief's "runs locally with `dotnet run`" is met (R12).** `dotnet run` at the
   repository root fails with a solution present (spiked). Plan: `dotnet run --project
   src/CommissionCalculator.Web` from the root, or `dotnet run` inside that folder.
   **Maintainer: "2: yes".**
3. **Data-model consequence of your "reject inconsistent data" answer**: an earlier-quarter deal
   listed both in the scenario's own deal list and in booking-quarter data must be identical in
   both, or the scenario is rejected (data-model.md, FR-004). Stated so you can see it; no payout
   depends on it.

## Complexity Tracking

No constitution violations to justify.
