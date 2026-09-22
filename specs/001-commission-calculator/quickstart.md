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
dotnet run --project tools/CommissionCalculator.Tools -- trace \
  --assemblies src/CommissionCalculator.Engine/bin/Debug/net10.0/CommissionCalculator.Engine.dll \
               src/CommissionCalculator.Web/bin/Debug/net10.0/CommissionCalculator.Web.dll \
  --results $(find . \( -name '*.trx' -path '*TestResults*' \) -o -path '*ci-evidence/*.json')
```

## Validation steps (each falsifiable)

1. **Fresh clone runs (SC-004).** Condition: a clean clone — evidence: `git status --ignored` shows
   no `bin/`/`obj/` immediately before running. Run the command above; expect `/` to return HTTP
   200 and the picker to list every file in `Scenarios/`. CI repeats this from a fresh checkout and
   additionally requests `/?scenario=tiers` (tasks T010, T060b).
2. **No network at run time (SC-004).** Condition: no network — evidence: `docker inspect` shows
   `NetworkMode: none` immediately before the request. CI's offline-smoke job runs the published app
   in `mcr.microsoft.com/dotnet/aspnet:10.0.12` with `--network none` and requests `/` (T035) and
   `/?scenario=tiers` (T060b) from a curl container sharing that network namespace; expect a rep
   table, and expect the same sidecar's request to an external host to fail. The result is a
   CI-evidence record, not a test (traceability "Verified by CI job").
3. **Every seeded amount matches the spec (SC-001).** Run the test command; expect the
   `SeededScenarios.feature` scenarios to pass and zero skipped (the run fails on any skip).
4. **Every line cites an FR (FR-003, SC-002).** Web tests assert every breakdown row's Rule cell is
   an FR ID present in spec.md; the traceability report's "Unexplained" gaps are empty.
5. **Rejected scenario (FR-004).** Select "Invalid data (rejected)"; expect an alert listing each
   error with its FR, and no rep tables (asserted by T048).
6. **Keyboard only (SC-005, FR-021).** Condition: no pointer input — evidence: the check is driven
   through the in-app browser with key presses only, and the recorded action log contains no pointer
   events. Tab to the picker, choose a scenario with arrow keys, submit with Enter, Tab through to
   each table; expect visible focus on every stop. Done in the UI phase (T033) and again in Polish
   (T064); the log excerpt goes in that phase's PR.
7. **Screen reader (FR-021) — maintainer.** Condition: VoiceOver on — evidence: a screenshot with
   the VoiceOver caption panel visible. Navigate by headings and tables; expect each rep's `h2`,
   the table caption and column headers announced with each cell. Done in the UI phase (T034) and
   again in Polish (T064); recorded on the PR.
