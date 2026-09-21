# CLAUDE.md

Rules that must fire while working, every turn. Gate-time governance lives in
`.specify/memory/constitution.md`; where the two differ, the constitution wins and this file is
fixed.

## Before saying something

- **Payout ambiguity is never mine to resolve.** If a reading of any rule could change what a rep
  is paid, stop and ask the maintainer with a worked dollar example, the options and a
  recommendation. No informed guesses, no "industry standard", no cap on how many such questions.
- **Check before asserting.** What a library does, what a file contains, what a change would touch:
  run the command or open the file in the same turn, or say the claim is unverified in the same
  sentence. "The framework already handles this" is a tell, not a source.
- **Absence needs a positive control.** Before reporting "not found", "can't be tested" or "no
  hits", show that the check would have seen the thing if it were there.

## While writing code and tests

- No implementation code before `/speckit-implement`. Inside it: write the test, run it, show it
  red, then implement.
- Every acceptance-scenario example is tested with its exact inputs and asserted to the cent.
- Never change a test to make it pass. If a test looks wrong, the spec is wrong: stop and say so.
- A failure-path test is watched failing with the guarded code removed; record the result.
- Every product test names the FR/SC it verifies; a tooling test names the principle it enforces.
  Every breakdown line in the UI cites its FR.
- Money is `decimal`, never `double`/`float`. The engine never reads the clock.
- Warnings are errors. No silent skips.

## Git and GitHub

- Commit only when asked. Push and merge only with approval for that specific action.
- After the ratification push, main changes only through a PR; one PR per tasks.md phase,
  squash-merged with branch deletion. No force-push, no `--no-verify`, no amend unless asked.
- Commit messages say why.
- Only `gh` account `gregoryschroeder-agentic` is used; never run `gh auth switch`. Never create a
  repository.

## Confidentiality (public repository)

- The private practice library is cited by number ("02") only — never copied or quoted at length.
  Only installer output under `.specify/` and `.claude/` may contain library-derived text.
- Before every push, search the working tree and the pushed commits' messages for a distinctive
  phrase from each library document read so far, and report zero hits (installer-output hits
  listed separately).
- Never write the home-directory path or an email address into a committed file.
