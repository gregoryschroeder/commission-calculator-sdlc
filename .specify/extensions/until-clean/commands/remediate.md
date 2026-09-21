---
description: "Fix every finding from the most recent analyze report, show each fix, commit, then re-run analyze — repeating until an analysis finds no issues. Runs automatically as the after_analyze hook."
---

## Why this exists

The maintainer answers every analysis the same way: fix all findings, then re-analyze, and keep going until
an analysis is clean. This command makes that the default rather than a request. It is invoked by the mandatory
`after_analyze` hook, so every `__SPECKIT_COMMAND_ANALYZE__` continues into it, and it ends by invoking
`__SPECKIT_COMMAND_ANALYZE__` again — whose hook invokes this again. The loop ends on a clean pass or on one of
the stop conditions below.

The analyze command's own closing question ("Would you like me to suggest concrete remediation edits…") is
superseded by this hook: do not wait for an answer to it. The analyze command is left unmodified because it is
upstream SpecKit content that an update would overwrite.

## Inputs

- **The analysis report immediately above** in the conversation — its findings table, coverage table and metrics.
- **The pass number**: count the `__SPECKIT_COMMAND_ANALYZE__` reports in the current chain (since the maintainer's last
  message that started it). The first analysis is pass 1.
- The feature directory, from `.specify/scripts/bash/check-prerequisites.sh --json --require-tasks --include-tasks`.

## Procedure

### 1. Stop if the analysis is clean

If the report has **zero findings** of any severity: output

```
## Analyze-until-clean: complete

Pass N found no issues. Passes run: N. Findings fixed across the chain: <total>.
```

and **stop**. Do not re-run analyze.

### 2. Stop conditions — check before fixing anything

Stop and report to the maintainer, **without** re-running analyze, if any of these hold:

- **Pass cap**: this is pass 8. Report the remaining findings; eight passes without converging means the fixes
  are generating findings, which is worth a person's attention.
- **No progress**: a finding in this report has the same subject — same requirement, task or artifact section,
  same defect — as one fixed in the previous pass. The fix did not stick or is oscillating; report both passes'
  versions of it.
- **Growth**: this pass has more CRITICAL + HIGH findings than the previous pass. Report which fixes introduced
  them.

### 3. Separate what needs the maintainer

A finding **needs the maintainer** when resolving it requires any of:

- choosing a value only the maintainer can set (a bound, a ceiling, a growth target, a cost);
- accepting a known shortfall against a requirement, or weakening a requirement;
- changing or overriding a decision the maintainer recorded (Clarifications, "maintainer's decision" notes);
- a constitution amendment;
- anything outward-facing or hard to reverse (repository settings, spending, publishing).

Ask all of these in **one** batch (the structured question tool, where the agent has one), with a recommended option first where there is a defensible
default, and wait. Never guess a maintainer decision to keep the loop moving. Record each answer where the
artifacts record decisions (spec Clarifications, dated) before continuing.

### 4. Fix every other finding

For each remaining finding, apply the recommendation — or a better fix, saying why — to the feature's
artifacts: `spec.md`, `plan.md`, `tasks.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`,
`checklists/`.

Rules that hold regardless of what a finding suggests:

- **Never make a finding disappear by weakening what it guards.** Narrowing a requirement to its true scope is a
  fix; deleting a guard, a demonstration or a coverage obligation to silence a finding is not.
- **Preserve history.** Where a fix changes a recorded claim, date it and say what it replaced, as the artifacts
  already do; never silently rewrite a decision record.
- **Verify before asserting.** If a fix depends on a fact about the repository or a vendor, check it in the same
  turn, with a positive control for any search whose empty result is meaningful.
- **Constitution conflicts are fixed in the artifacts**, never by editing the constitution (that needs the
  maintainer — section 3).
- **Stay inside the feature directory**, except for a task that must reference another feature, which is recorded,
  not edited, unless the finding is explicitly about that file.

### 5. Show the fixes

Output:

```
## Remediation — pass N

| ID | Severity | Fix applied | Where |
|----|----------|-------------|-------|
```

one row per finding, including those resolved by a maintainer answer (say so). Keep each "Fix applied" to a
sentence: what changed, not a restatement of the finding.

### 6. Commit

Commit the changed artifacts on the current branch — never push — with a message naming the pass and the finding
IDs, explaining why in the body, ending with the attribution lines the session requires. One commit per pass, so
each pass's fixes are reviewable on their own.

### 7. Re-analyze

Invoke `__SPECKIT_COMMAND_ANALYZE__`. Its `after_analyze` hook invokes this command again, which starts at step 1.
