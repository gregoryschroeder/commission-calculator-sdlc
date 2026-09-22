# Commission Calculator

A local web app that computes quarterly sales commission for a SaaS sales team from seeded
scenarios, and shows every rep's statement line by line, each line citing the requirement (FR)
that produced it.

The requirements, decisions and expected amounts live in
[`specs/001-commission-calculator/spec.md`](specs/001-commission-calculator/spec.md); Appendix A
holds the full expected statement for every seeded scenario.

## Run

Requires the .NET 10 SDK (`global.json` pins 10.0.400 and rolls forward within 10.0).

```bash
dotnet run --project src/CommissionCalculator.Web
```

or, from inside `src/CommissionCalculator.Web`, plain `dotnet run`. Then open the URL it prints.

Scenarios are read from the `Scenarios` folder next to the built app. To read them from somewhere
else, pass the optional setting `Scenarios:Directory` (added with the scenario catalog in
implementation Phase 2):

```bash
dotnet run --project src/CommissionCalculator.Web -- --Scenarios:Directory=/path/to/scenarios
```

## Test

```bash
dotnet build CommissionCalculator.slnx -warnaserror
dotnet test --solution CommissionCalculator.slnx --fail-skips on --report-trx --coverage --coverage-output-format cobertura
tools/ci/coverage-gate.sh 80
tools/ci/isolated-tests.sh
```

Skipped tests fail the run, the engine's line coverage must be at least 80%, and every test must
also pass when run on its own.

## Traceability

Every requirement is traced to the member that implements it and the tests that cover it:

```bash
dotnet run --project tools/CommissionCalculator.Tools -- trace \
  --assemblies src/CommissionCalculator.Engine/bin/Debug/net10.0/CommissionCalculator.Engine.dll \
               src/CommissionCalculator.Web/bin/Debug/net10.0/CommissionCalculator.Web.dll \
  --results TestResults/*.trx
```

The committed output is
[`specs/001-commission-calculator/traceability.md`](specs/001-commission-calculator/traceability.md).
Any FR or SC with no test or no member is listed as a gap; a gap is "Explained" only where the
report gives its reason (scope, evidence-only, CI evidence, manual evidence), and `--strict` fails
on any other.

## CI

`.github/workflows/ci.yml` runs the steps above and three more jobs: **smoke**, which starts the
app from a clean checkout and requests a seeded scenario; **offline-smoke**, which serves the
published app in a container with `--network none` and proves the same page renders with no
network; and **traceability**, which re-runs the trace over the other jobs' artifacts with
`--strict`.

## Layout

- `src/CommissionCalculator.Engine` — the commission rules; no I/O, no clock, exact `decimal` money.
- `src/CommissionCalculator.Web` — Razor Pages view of the engine's statements.
- `tests/` — unit tests (xUnit v3) and Gherkin feature files (Reqnroll) mirroring the spec's
  acceptance scenarios.
- `tools/` — CI helpers: coverage gate, test lister and the traceability generator.
