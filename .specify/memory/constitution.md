<!--
AMENDMENT LOG (newest first; earlier entries are kept, marked superseded, never deleted)

## v1.1.1 — 2026-09-22 — C7 confirmed; the approved reading of `dotnet run`
- Version change: 1.1.0 → 1.1.1 (PATCH: an assumption confirmed by evidence and an approved
  interpretation written down; no principle or constraint changes what it permits).
- Modified: C7 (assumed → confirmed, with a new revisit trigger); C2 and Principle VII (record the
  maintainer's reading of "runs with `dotnet run`"); the pairwise note on II × C7.
- Rationale: the first CI run (2026-09-22, run 35758821698, PR #2) resolved `global.json` to SDK
  10.0.401 on GitHub Actions and passed every gate, so C7's "assumed at ratification" is now
  measured. The same run carried GitHub's notice that `ubuntu-latest` moves to Ubuntu 26 from
  2026-10-19, which becomes C7's revisit trigger. `dotnet run` at the repository root cannot start
  an app when a solution file is present (spiked 2026-09-21, research R12); the maintainer approved
  `dotnet run --project src/CommissionCalculator.Web` (or plain `dotnet run` inside that folder) at
  the plan gate on 2026-09-21, and the constitution now says so. Docker, used only by the
  offline-smoke CI job, stays assumed until that job first runs (tasks T036).
- Deferred: none.

## v1.1.0 — 2026-09-21 — tooling tests and read-only repository files (superseded by v1.1.1)
- Version change: 1.0.0 → 1.1.0 (MINOR: guidance in II and III materially expanded — it changes
  what a compliant test may do and what it must name; no principle removed or redefined).
- Modified: II (per-test independence wording), III (tests of project tooling).
- Rationale: analyze pass 2 on feature 001 found two conflicts inside this document that the
  ratification pairwise check missed. (1) II requires a test before any real logic, including CI
  tooling (coverage gate, test lister, traceability generator), while III required every test to
  name an FR/SC — tooling implements no FR, so the two could not both hold. (2) II said every test
  passes "against nothing it did not create", while SC-001 of feature 001 requires tests that read
  the shipped seed files and spec.md. Both resolutions were chosen by the maintainer (2026-09-21):
  tooling tests name the principle they enforce; reading committed repository files is allowed,
  mutable shared state is not.
- Pairwise check re-run for the changed text (see the check below, entries dated 2026-09-21,
  v1.1.0).
- Deferred: none.

## v1.0.0 — 2026-09-21 — initial ratification (superseded by v1.1.0)
- Version change: (template) → 1.0.0
- Principles added: I Payout Ambiguity Is the Maintainer's Decision (NON-NEGOTIABLE);
  II Test-First, Exact to the Cent (NON-NEGOTIABLE); III Spec Fidelity and Traceability;
  IV Evidence Before Assertion; V Clean Architecture and Warnings-as-Errors;
  VI Accessible by Default; VII The Runtime Environment Is Part of the Feature.
- Sections added: Technology & Scope Constraints (with provenance, revisit triggers and the
  pairwise constraint check); Development Workflow & Quality Gates; Governance.
- Rationale: this project computes money owed to people. The failure that matters is a payout
  that is wrong, or right for a reason nobody chose. The two NON-NEGOTIABLE principles raise the
  bar past SpecKit's defaults at exactly those two points: who decides an ambiguous rule, and
  whether a stated example is actually tested.
- Deviation from the SpecKit constitution command, deliberate: the command treats its Sync Impact
  Report as scratch to delete before commit. This project keeps it as a permanent amendment log at
  the top of the file (library 01), so the governance history reads top to bottom.
- Deferred: none.
-->

# Commission Calculator Constitution

## Core Principles

### I. Payout Ambiguity Is the Maintainer's Decision (NON-NEGOTIABLE)

Any ambiguity whose resolution changes what any rep is paid — by any amount, in any scenario — is
decided by the maintainer, in writing, and never by an assumption, a default, an "industry
standard" or an informed guess. This overrides two SpecKit defaults for such ambiguities: the
specify step's instruction to make informed guesses, and its cap of three
`[NEEDS CLARIFICATION]` markers. Every payout-affecting ambiguity gets a marker, however many there
are, and clarify is re-run past its per-session question cap until a run finds none left.

Each question put to the maintainer shows a worked scenario in which the readings produce
different dollar amounts, the options, and a recommendation. The maintainer's answer is recorded in
`spec.md` verbatim. An ambiguity discovered later — in planning, in implementation, or by a test —
stops the work and goes back to the maintainer; it is never resolved in code.

Ambiguities that cannot change a payout (layout, wording, which scenario is selected by default)
follow SpecKit's normal defaults.

*Rationale:* a commission rule written in prose has several defensible readings, and each reading
is a different paycheck. An agent's "reasonable" choice is still a compensation-policy decision
made by someone with no authority to make it.

### II. Test-First, Exact to the Cent (NON-NEGOTIABLE)

- Red-green-refactor without exception: a test is written and watched failing before the code that
  makes it pass. No implementation code is written before `/speckit-implement`.
- Every concrete example in an acceptance scenario has a test that exercises that exact example —
  the same inputs, and the expected amount asserted to the cent. A representative or "equivalent"
  substitute does not satisfy this. If an example turns out to be unachievable as written, that is a
  spec defect and goes back to the maintainer (Principle I).
- Unit tests use the test framework's own idiom. Integration tests are Gherkin feature files whose
  scenarios mirror `spec.md`'s acceptance scenarios; the BDD tool is confirmed to be currently
  maintained, with the date and source recorded, before it is adopted.
- A test guarding a failure path (rejected input, refund clawback, draw recovery) is watched failing
  with the guarded behaviour removed, and the observed result is recorded.
- A test is never changed to make it pass. A test that looks wrong means the spec is wrong: stop and
  raise it with the maintainer.
- No false greens: no silent or conditional skips; the suite reports every test it did not run, by
  name and reason, and CI fails on any skip. Every test passes run alone and in any order, against
  no mutable shared state it did not create; reading files committed to the repository (seed
  data, the spec) is allowed.
- Coverage is reported on every CI run. 100% is the aim; below 80% line coverage of the engine is
  a defect. Coverage is a by-product: a test that asserts nothing is forbidden.

*Rationale:* a payout that is off by a cent in one tier boundary is invisible in a happy-path test
and obvious in an exact one. The examples in the spec are the maintainer's own statement of what
correct means; testing an easier substitute quietly narrows it.

### III. Spec Fidelity and Traceability

- Every change that alters calculated payouts, what a test proves, or the application's behaviour
  goes through the SpecKit pipeline (specify → clarify → plan → tasks → analyze → implement).
  Scope is decided by stakes, not by calling something a fix; an exemption granted once is not a
  precedent.
- Clarify and analyze are mandatory for every feature. Analyze runs until a pass is clean or the
  remediate loop reaches a stop condition.
- Every number in a spec is tagged as a requirement or a placeholder to be measured.
- Every functional requirement (FR) and success criterion (SC) traces to the member that implements
  it and to the tests that verify it. Every test of product behaviour names the FR/SC it verifies;
  a test of project tooling (CI gates, generators) names the constitution principle it enforces
  instead, and the traceability report lists such tests in their own section. Every line of a
  payout breakdown shown to a user cites the FR that produced it. The trace is generated from test
  metadata by a script, never maintained by hand, and any FR/SC with no test or no implementation is
  listed as a gap.

*Rationale:* a payout the user cannot trace to a rule cannot be checked, and a trace kept by hand
drifts from the code the first time nobody updates it.

### IV. Evidence Before Assertion

- Every load-bearing claim in `research.md`, and every fact in this constitution about the world
  outside the repository, records how it was established — measured, spiked, decompiled, read from
  the vendor's current documentation (with the date consulted), assumed, or copied from a named
  source and not independently verified — and when. A claim does not gain authority by being moved
  into a more trusted document; a hedge stays a hedge unless new evidence is recorded with it.
- Perishable claims carry a revisit trigger.
- A claim that something is absent, impossible, or untestable meets the same bar as a claim that it
  is present, and needs a positive control.
- This applies in conversation as well as in documents: a statement about what a library does, or
  what a change would touch, is checked in the same turn or stated with its uncertainty.

*Rationale:* the fidelity gates check that artifacts agree with each other, not that they are true.
Evidence is the only check on truth, and it has to be visible on the page to the next reader.

### V. Clean Architecture and Warnings-as-Errors

- The commission engine is a plain library with no dependency on the web framework, UI, file system
  or clock. The dependency rule points inward: the web app depends on the engine, never the
  reverse. The UI renders the engine's breakdown and never recomputes or adjusts an amount.
- Money is represented exactly (no binary floating point anywhere a monetary amount is held or
  computed). Where rounding happens and to what precision is a spec decision under Principle I.
- Warnings are build errors, identically locally and in CI, from the first commit.
- SOLID and DRY apply; a trade-off against a SOLID principle is flagged, not silently resolved.
  No speculative abstractions, configuration or error handling for cases the spec does not require.
  Input is validated at the boundary (seeded scenario data); internal code trusts its callers.

*Rationale:* the engine is the thing whose correctness matters; keeping it free of framework
concerns is what makes it exhaustively unit-testable and lets the UI be a thin, honest view of it.

### VI. Accessible by Default

The UI meets WCAG 2.2 AA as a baseline: semantic HTML, a label for every control, full keyboard
operation with visible focus, sufficient colour contrast, data tables with header cells, and no
information conveyed by colour alone. An accessibility gap in UI being touched is a defect.

*Rationale:* the breakdown is the product; a breakdown a screen-reader user cannot navigate is a
breakdown that does not exist for them.

### VII. The Runtime Environment Is Part of the Feature

The application's runtime environment is a developer machine running `dotnet run --project
src/CommissionCalculator.Web` (or plain `dotnet run` inside that folder; approved reading of
"runs with `dotnet run`", plan decision 2, 2026-09-21). Any change that
makes startup or a request path depend on something new — an environment variable, a file, a
service, a port — is an explicit task that wires it into every place the app is run (the run
instructions and CI), or the task list states in one line that the feature adds none. No degraded
window (cold start, pause, reclaimed instance) is accepted, because nothing is deployed; a feature
that introduces one must map it to an FR or SC.

*Rationale:* "works on the machine that wrote it" is not the same as "runs with `dotnet run` from a
fresh clone", and only an explicit task keeps them the same.

## Technology & Scope Constraints

Each external fact carries how it was established, when, and what would make it wrong.

| # | Constraint | Established | Revisit when |
|---|---|---|---|
| C1 | Target .NET 10 (LTS). It is the latest stable .NET: channel 10.0 is `active` LTS (EOL 2028-11-14); 11.0 is a release candidate (`11.0.0-rc.1`, `go-live`). | Vendor metadata: Microsoft `releases-index.json`, consulted 2026-09-21. Build SDK 10.0.400 measured with `dotnet --version`. | Microsoft's release index shows 11.0 with support phase `active`; or 2028-11-14. |
| C2 | One self-contained solution that runs locally with `dotnet run` — read as `dotnet run --project src/CommissionCalculator.Web` from the root, or plain `dotnet run` inside that folder, because `dotnet run` cannot start an app from a solution root (spiked 2026-09-21, research R12; reading approved by the maintainer 2026-09-21). No external services at runtime, no authentication, no database; data is in memory or a local file in the repository. | Maintainer's requirement (project brief, 2026-09-21). | Maintainer changes scope. |
| C3 | Monetary amounts are USD. Tax, currency conversion and multi-year contract handling are out of scope. | Maintainer's requirement (project brief, 2026-09-21). | Maintainer changes scope. |
| C4 | The repository is public (`gregoryschroeder/commission-calculator-sdlc`); the account in use has WRITE, not admin, permission, so branch protection and auto-delete-on-merge cannot be configured from here. | Measured: `gh repo view --json visibility,viewerPermission`, 2026-09-21 (PUBLIC, WRITE, empty). | Permission or visibility changes; re-check before relying on either. |
| C5 | The private practice library is cited by document number only. Its documents are never copied into this repository or quoted at length. The only library-derived text committed is what its installer generates under `.specify/` and `.claude/`. | Maintainer's requirement (project brief, 2026-09-21). | Library visibility changes. |
| C6 | Pipeline tooling is SpecKit 1.0.9 with the library's kit installed (commit recorded in `docs/PROVISIONING.md`). | Measured: `specify version`, `install.sh --check` exit 0, 2026-09-21. | SpecKit upgrade — re-run the installer and `--check`. |
| C7 | CI runs on GitHub Actions for this repository. Test-only dependencies (test framework, BDD tool, coverage tool) are pinned package references restored by `dotnet`; none is a runtime dependency. | Measured: the first CI run (2026-09-22, run 35758821698, PR #2) installed SDK 10.0.401 from `global.json` via `setup-dotnet@v6` on `ubuntu-latest` and passed every gate. Package choices and their maintenance status are established in `research.md`. | The `ubuntu-latest` label moves to Ubuntu 26 from 2026-10-19 (GitHub annotation on that run) — re-check the first CI run after that date; any package's maintenance status changes. |

**Library documents 07 and 08 (background).** 07 applies as a scope decision: requirements live
in the repository (spec per feature, tasks, traceability) because this is a solo project with no
non-technical stakeholder who would need a separate tracking tool; and the Given/When/Then format
that SpecKit already enforces is not restated here. 08 largely does not apply, because nothing is
deployed: there is no deploy pipeline to keep honest and no accepted infrastructure trade-off. The
one part retained is in Principle VII — a new runtime dependency is a task, and a degraded window
would need an FR or SC.

**Pairwise constraint check (2026-09-21).** Every pair of constraints and principles was asked
whether both can hold at once. Pairs that needed an answer:

- C2 (no external services) × C7 (GitHub Actions CI): compatible — CI is not a runtime dependency
  of the application.
- C4 (public repo) × C5 (library not copied): in tension, because the installer-generated
  template overrides carry library-derived text into a public repository. Resolved by the
  maintainer's explicit exception for installer output only; every push is preceded by a search of
  the tree and the commit messages for distinctive library phrases, with installer-generated hits
  listed separately.
- C4 (no branch protection) × Workflow (every change after ratification reaches main through a
  PR): compatible but enforced by rule, not by the platform. Recorded so nobody reads a missing
  protection rule as permission.
- Principle I (every payout ambiguity asked) × SpecKit clarify's 5-question cap: compatible by
  re-running clarify until a run finds nothing payout-affecting.
- Principle II (tests before code) × "no implementation code before `/speckit-implement`":
  compatible — tests are written inside `/speckit-implement`, each phase's tests shown failing
  first.
- Principle II (exact examples) × III (numbers tagged): compatible — an amount in an acceptance
  example is a requirement by definition.
- Principle II (no skips; CI fails on a skip) × C7 (test tools restored by `dotnet`): compatible —
  no test depends on a tool the runner lacks; CI installs the pinned SDK (confirmed by the first CI
  run, 2026-09-22, SDK 10.0.401).
- Principle V (engine has no clock) × dates in the rules (close/booking/start dates, quarters):
  compatible — dates are inputs, never read from the system clock.
- Principle VI (accessibility) × C2 (no external services): compatible — no CDN assets are
  needed; styles are local.

No pair was found that cannot hold.

**Pairwise check, v1.1.0 (2026-09-21).** Missed at ratification and found by analyze pass 2:

- Principle II (a test before any real logic, including tooling) × Principle III (every test
  names an FR/SC): could not both hold for tooling tests. Resolved by III's tooling clause.
- Principle II (tests against nothing they did not create) × feature SC-001 (tests read the
  shipped seed files): could not both hold. Resolved by II's committed-files clause.
- Re-checked against the new text: III's tooling clause × III's generated trace (compatible —
  the report lists tooling tests separately); II's committed-files clause × II's per-test
  isolation in CI (compatible — committed files are read-only, so no order dependence arises).

## Development Workflow & Quality Gates

- **Approval per action.** Commit only when the maintainer asks. Push, and merge a PR, only with
  explicit approval for that specific action; an earlier approval does not carry forward.
- **Main is reached only through a pull request** after the initial ratification push. Every
  tasks.md phase is its own branch and PR; no PR spans two phases, and no phase starts before the
  previous one is merged. Merges are squash-merges, deleting the branch.
- **CI is the review gate.** Solo project, so the pipeline is the reviewer: warnings-as-errors
  build, the full suite with no silent skips, coverage reported. Nothing merges on red. A red run on
  a change that "cannot" have broken anything is investigated as a finding, not retried as flake.
- **No history rewriting.** Prefer a new commit to an amend; no force-push, no `--no-verify`,
  unless the maintainer asks for that specific action.
- **Ask before consequential or hard-to-reverse decisions**, and not about things with an obvious
  answer. A pattern that seems to follow from an earlier approval is named and asked about, not
  assumed.
- **Pre-push confidentiality check** (C5): before every push, the working tree and the messages of
  the commits being pushed are searched for a distinctive phrase from each library document read so
  far; zero hits outside installer output is required, and installer-output hits are listed.
- **Stage gates.** The maintainer approves ratification, the spec after clarify, the plan, the
  analyzed task list (planning artifacts merge as their own PR before implementation), each
  implementation phase's merge, and the traceability report.

## Governance

This constitution supersedes other practice for this repository. `CLAUDE.md` restates the rules
that must fire while working rather than at a gate; where the two differ, this document wins and
`CLAUDE.md` is corrected.

Amendments: state the rationale first; decide the version bump (MAJOR — a principle removed or
redefined, or a NON-NEGOTIABLE made optional; MINOR — a principle added or guidance materially
expanded, including a material change to Technology & Scope Constraints; PATCH — wording only) and
reason it through when ambiguous; prepend an entry to the amendment log; tag any new external fact
and re-run the pairwise check for any new constraint; update the version line. An amendment follows
the same approval and PR rules as any other change. An exception requested to legitimise work
already in flight must show compliance is impossible, verified rather than asserted.

Compliance review: every plan's Constitution Check, and every analyze pass, reads this document as
a subject as well as an authority — untagged external claims, expired revisit triggers and
conflicting constraints are findings.

**Version**: 1.1.1 | **Ratified**: 2026-09-21 | **Last Amended**: 2026-09-22
