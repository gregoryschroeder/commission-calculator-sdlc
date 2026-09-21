# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]

**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION]

**Primary Dependencies**: [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION]

**Storage**: [if applicable, e.g., PostgreSQL, CoreData, files or N/A]

**Testing**: [e.g., pytest, XCTest, cargo test or NEEDS CLARIFICATION]

**Target Platform**: [e.g., Linux server, iOS 15+, WASM or NEEDS CLARIFICATION]

**Project Type**: [e.g., library/cli/web-service/mobile-app/compiler/desktop-app or NEEDS CLARIFICATION]

**Performance Goals**: [domain-specific, e.g., 1000 req/s, 10k lines/sec, 60 fps or NEEDS CLARIFICATION]

**Constraints**: [domain-specific, e.g., <200ms p95, <100MB memory, offline-capable or NEEDS CLARIFICATION]

**Scale/Scope**: [domain-specific, e.g., 10k users, 1M LOC, 50 screens or NEEDS CLARIFICATION]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

[Gates determined based on constitution file]

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

## Standing Gates *(agentic-sdlc kit)*

These two rows are part of the Constitution Check above and MUST be filled in for every plan. They
cite the constitution's principles by subject, because principle numbers differ per project; name
the matching principle in the Notes column.

| Gate | Status | Notes |
|---|---|---|
| Provenance | | Every research.md Decision is tagged with how it was established: decompiled / spiked / measured / vendor-doc + date consulted / assumed / copied from X, not independently verified. No assumed or copied claim is load-bearing for correctness by the time tasks are generated. |
| Degraded window | | Each newly accepted degraded window maps to an FR or SC in spec.md, or N/A with the reason. |

<!--
  PROVENANCE. A claim reached by recall and written in a research register passes both fidelity
  gates, because they check drift, not truth. The tell for recall is an appeal to convention ("the
  framework already handles this"); the tell for a copy is that nobody in the session can say how
  it was established. A claim does not gain authority by being moved into this plan.

  DEGRADED WINDOWS. If any constraint accepts a cost tradeoff that creates a degraded window --
  scale-to-zero, auto-pause, a cold start, a reclaimed instance -- state for each:
    - what a caller experiences during the window
    - the bound past which "degraded" becomes "down"
    - how an occurrence is recorded
  and name the FR or SC each maps to, so a missing implementation task is caught by the analyze
  coverage pass rather than by a caller. A window accepted by an earlier feature needs restating
  only where this feature's behaviour depends on it.
  Why: a plan once filed a database tier's idle pause beside an already-accepted scale-to-zero
  decision as "the same category of tradeoff" and stopped. The resume did not fit inside the client
  driver's default connection timeout, by about a second, and that gap became an entire follow-up
  feature.

  QUICKSTART.md -- make each verification step falsifiable:
    - Behaviour that exists only under a condition: capture evidence the condition held
      immediately before the observation, and record it with the result.
    - A requirement of the form "record when X happens": verify by producing X and reading the
      record, never by reasoning that a logging path exists.
  These are prompts for whoever writes quickstart.md; the enforceable form of both lives in
  tasks.md, because the analyze step loads spec.md, plan.md and tasks.md but never quickstart.md.
-->
