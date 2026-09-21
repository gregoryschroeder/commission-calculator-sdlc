# Quickstart & validation: Quarterly Sales Commission Calculator

## Prerequisites

.NET SDK 10.0.4xx (`global.json` pins 10.0.400, rolling forward to later 10.0 feature bands).
No other services, accounts or network access at run time.

## Run

```bash
dotnet run --project src/CommissionCalculator.Web
```

(or `dotnet run` from inside `src/CommissionCalculator.Web`), then open the URL it prints.

## Test

```bash
dotnet build -warnaserror
dotnet test --fail-skips on --report-trx --coverage --coverage-output-format cobertura
dotnet run tools/Traceability.cs -- $(find . -name '*.trx' -path '*TestResults*')
```

## Validation steps (each falsifiable)

1. **Fresh clone runs (SC-004).** Condition: a clean clone with no `bin/`/`obj/` — evidence:
   `git status --ignored` shows none before step. Run the command above; expect the page at `/`
   to return HTTP 200 and list every file in `Scenarios/` in the picker. CI's smoke step does the
   same from a fresh checkout.
2. **Every seeded amount matches the spec (SC-001).** Run the test command; expect the
   `SeededScenarios.feature` scenarios to pass and zero skipped (the run fails on any skip).
3. **Every line cites an FR (FR-003, SC-002).** Web tests assert every breakdown row's Rule cell is
   an FR ID present in spec.md; the traceability report lists no FR without a test.
4. **Rejected scenario (FR-004).** Select "Invalid data (rejected)"; expect an alert listing each
   error with its FR, and no rep tables.
5. **Keyboard only (SC-005, FR-021).** Condition: pointer not used — evidence: perform the step
   with the trackpad disabled or untouched and note it. Tab to the picker, choose each scenario
   with arrow keys, submit with Enter, Tab through to each table; expect visible focus on every
   stop and every rep's table reachable. Record the result in the PR for the UI phase.
6. **Screen reader (FR-021).** With VoiceOver on (evidence: VoiceOver caption panel visible),
   navigate by headings and tables; expect each rep's `h2` and table caption announced and the
   table's column headers read with each cell. Record the result in the PR for the UI phase.
