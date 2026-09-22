# Feature Specification: Quarterly Sales Commission Calculator

**Feature Branch**: `001-commission-calculator`

**Created**: 2026-09-21

**Status**: Clarified; approved by the maintainer 2026-09-21 (Appendix A added at plan decision 1A)

**Input**: User description, verbatim (the project brief, everything above its PROCESS section):

```text
Build a sales commission calculator as a standalone web app.

CONTEXT
Quarterly commission for a SaaS sales team.

COMPENSATION RULES
1. Each rep has a quarterly quota in USD.
2. Commission rate is tiered on quota attainment: 5% up to 100% of quota,
   8% from 100% to 150%, and 12% above 150%.
3. Reps who start mid-quarter have a prorated quota.
4. A deal may be split between multiple reps, with split percentages summing
   to 100%.
5. Reps receive a monthly draw of $4,000, recoverable against earned commission.
6. If a deal is refunded, the commission on that deal is clawed back.
7. Deals have both a close date and a booking date.
8. Deal amounts are in USD. Ignore tax, currency conversion, and multi-year
   contract handling.

DELIVERABLES
- A web app that runs locally.
- A scenario picker with seeded scenarios.
- For the selected scenario, each rep's commission with a line-by-line
  breakdown showing which rule produced each amount.
- Tests.

CONSTRAINTS
Latest stable .NET (.NET 10). One self-contained solution that runs locally
with `dotnet run`. No external services, no auth, no database beyond in-memory
or a local file.

REPOSITORY
An empty public GitHub repository, gregoryschroeder/<the name of this
directory>, already exists, and the active gh account
(gregoryschroeder-agentic) has push access to it. Publish the work there. Do
not create any other repository, and do not run `gh auth switch`.

TRANSCRIPT
The last thing you do is add this session's Claude Code transcript to the
repository under transcript/ and push it.
- Source: this session's .jsonl in the ~/.claude/projects/ folder for this
  directory, plus <session-id>/subagents/*.jsonl if there are any. Do not copy
  <session-id>/tool-results/.
- Commit a redacted copy of each .jsonl and a readable Markdown rendering of it.
- Redact in every field, including cwd and toolUseResult: the content of
  anything that came from outside this directory (files read, instruction
  files injected into context, command output that prints either), replaced by
  a one-line note naming what was removed; my home directory path; email
  addresses; anything resembling a credential.
- Prove it before committing: search the redacted files for the home path, for
  email-address patterns other than noreply addresses, and for a distinctive
  phrase from each outside file you read, and report zero hits.
The transcript necessarily ends at this step; say so in transcript/README.md.
```

## Clarifications

Answers are the maintainer's, recorded verbatim; the option each answer selected follows it.

### Session 2026-09-21

- Q: When a rep's attainment goes past a tier boundary, does each slice of bookings earn its own
  tier's rate, or does the highest tier reached apply to all of the rep's bookings? (FR-006)
  → A: "a" — Option A, marginal: each slice of credited bookings earns its band's rate (5% up to
  100% of quota, 8% on 100–150%, 12% above 150%).
- Q: Which date puts a deal in the quarter for crediting: its close date or its booking date?
  (FR-008) → A: "b" — Option B, booking date: a deal counts in the quarter containing its booking
  date.
- Q: On a split deal, does each rep's attainment count only their split share of the deal, or the
  full deal amount? (FR-012) → A: "a" — Option A, share only: a rep's split share counts toward
  both their attainment and their commission.
- Q: How should a mid-quarter starter's quota be prorated: by calendar days, or by months?
  (FR-010) → A: "a" — Option A, calendar days: full quota × (days from start date to quarter end,
  both inclusive) ÷ (days in quarter).
- Q: When a deal is refunded, how is "the commission on that deal" measured for the clawback?
  (FR-016) → A: "a" — Option A, recompute the quarter without the refunded deal (it no longer
  counts toward attainment); the clawback is the difference.
- Q: If a rep's earned commission for the quarter is less than the draws they were paid, what
  happens to the shortfall? (FR-015) → A: "a" — Option A, carry forward: unrecovered draw becomes
  a balance recovered from future commission; each rep has an opening balance and the statement
  shows the closing balance. The amount payable is never negative.
- Q: How much draw does a rep who starts mid-quarter receive for the quarter? (FR-014) → A: "b" —
  Option B, $4,000 per month, with the start month prorated by calendar days employed ÷ days in
  that month.
- Q: Where are amounts rounded to the cent, and how is a half-cent rounded? (FR-018) → A: "a" —
  Option A, round each computed amount (prorated quota, split share, prorated draw, each tier
  line, clawback) to the cent when it is produced, half away from zero; later steps use the
  rounded value, and totals are sums of the lines shown.
  *(Note, not part of the answer: for split shares this is superseded by the later
  largest-remainder answer below; FR-012 and FR-018 reflect the later answer.)*
- Q: Which quarter's statement carries a clawback: the quarter the deal was booked in, or the
  quarter the refund happens in? (FR-016) → A: "b" — Option B, clawback appears in the quarter
  containing the refund date, computed from the booking quarter's data; a negative earned
  commission adds to the recoverable balance.
- Q: Can a deal be refunded in part, and if so, how is the clawback sized? (FR-017) → A: "b" —
  Option B, partial refunds allowed: recompute the booking quarter with the deal amount reduced
  by the refunded amount; the clawback is the difference.
- Q: When a split deal's shares don't round to whole cents, how is the leftover cent assigned so
  the shares add back up to the deal amount? (FR-012, FR-018) → A: "c" — Option C, largest
  remainder: round every share down to the cent, then give leftover cents one at a time to the
  shares with the largest fractional remainders (ties to the first-listed rep).
- Q: If a deal's booking date is before the start date of a rep credited on it, what happens to
  that rep's credit? (FR-011) → A: "a" — Option A, reject the scenario, naming the deal and rep.
- Q: If a scenario lists a rep whose start date is after the quarter's last day, is the scenario
  rejected, or is the rep shown with nothing earned? (FR-011) → A: "a" — Option A, reject the
  scenario, naming the rep.
- Q: Can one deal be refunded more than once, for example in two partial refunds in different
  quarters? If so, how is each clawback sized? (FR-017) → A: "b" — Option B, multiple refunds
  allowed, applied in refund-date order; each clawback is the booking quarter's commission with
  the earlier refunds applied minus the same with this one also applied. The refunds may not total
  more than the deal.
- Q: When several different deals from the same booking quarter are refunded, is each clawback
  sized after the earlier refunds on other deals, or each against the untouched quarter?
  (FR-016, FR-017) → A: "a" — Option A, order all of a rep's refunds against deals from the same
  booking quarter by refund date (ties in listed order); each clawback is that quarter's
  commission with all earlier refunds applied minus the same with this one also applied.
- Q: When a split deal is partly refunded, is each rep's remaining credit their original share
  minus their share of the refund, or a fresh largest-remainder split of the reduced deal amount?
  (FR-012, FR-017) → A: "b" — Option B, after each refund, the reduced deal amount is re-split
  across reps by largest remainder (FR-012), and each rep's clawback is computed from their
  re-split credit.
- Q: Does the "deal booked before a credited rep's start date" rejection apply to every deal in
  the scenario, or only to deals that count toward this quarter? (FR-011) → A: "b" — Option B,
  apply the start-date check to every deal in the scenario, counted or not.
- Q: When earlier-quarter data supplied to size a clawback includes a split deal shared with a rep
  who isn't on this quarter's roster, is that allowed, or is the scenario rejected? (FR-016, Edge
  Cases) → A: "a" — Option A, allow reps who appear only in booking-quarter data, with their start
  dates; they get no statement. The "rep not in scenario" rejection applies to this quarter's
  deals only.
  *(Note, not part of the answer: "this quarter's deals" means the scenario's own deal list,
  counted or not — made explicit by the later answer on inconsistent inputs; FR-004 reflects it.)*
- Q: If re-splitting a refunded split deal gives a rep more credit than before, so their clawback
  computes as a negative amount, is it shown as a negative clawback or floored at $0.00? (FR-017)
  → A: "a" — Option A, a clawback is exactly the recomputed difference and may be negative (shown
  as a negative clawback line).
- Q: Should these malformed inputs reject the scenario: amounts that aren't whole cents, a
  negative opening recoverable balance, and a 0% split credit? (FR-004, FR-005, FR-012, FR-015)
  → A: "a" — Option A, reject all three: monetary inputs must be whole cents; opening balance ≥
  $0.00; split percentage > 0% and ≤ 100%.
- Q: Should each of these inconsistent inputs also reject the scenario — the same rep listed twice
  on one deal; a prorated quota that rounds to $0.00; a deal booked outside the quarter credited to
  a rep not on the roster; missing, incomplete or malformed booking-quarter data for a refund in
  this quarter; a rep whose start date in booking-quarter data differs from the roster's? (FR-004,
  FR-010, FR-012, FR-016) → A: "a" — Option A, reject all five, each with a message naming the
  rep, deal or quarter involved.

### Session 2026-09-21 (analyze)

- Q: US6 AS7 and AS9 state re-split shares ($4,950.10/$4,950.09, C's $0.01) but no statement line
  shows them. How should they become testable? (FR-003, FR-017) → A: "Add a re-split line
  (Recommended)" — after each refund on a split deal, the rep's statement shows a line with their
  re-split share citing FR-017; Appendix A.9 gains those lines (no payout changes).

### Session 2026-09-22 (analyze)

- Q: The data model requires unique scenario ids, unique repIds on the roster and unique dealIds,
  but FR-004 doesn't say so. A duplicate repId could change who is credited. What should happen?
  (FR-004) → A: "Reject the scenario (Recommended)" — a duplicate repId on the roster or a
  duplicate dealId within one quarter's deal list rejects the scenario, and two scenario files with
  the same id are a load error, each naming the duplicate.
- Q: FR-004 rejects booking-quarter data that is "incomplete" — including a missing deal or a
  missing earlier refund. The engine can't know a deal was left out, and an omitted deal changes
  the clawback. How should completeness be defined? (FR-004, FR-016) → A: "Detectable +
  precondition (Recommended)" — the engine rejects what it can detect: a refunded earlier-quarter
  deal with no booking-quarter data, a credited roster rep with no rep entry, a partner with no
  start date. "All deals / all earlier refunds are included" becomes a stated precondition on
  scenario data, enforced for the shipped seeds by the seeded-scenario check (seeds must equal
  Appendix A).

## User Scenarios & Testing *(mandatory)*

The user is anyone checking a quarterly commission statement: a sales-operations analyst, a
manager, or a rep. They pick one of the seeded scenarios and read, for every rep in it, what the
rep is paid and exactly which rule produced every amount.

Every number in an acceptance scenario below — dollar amounts, percentages, dates and day
counts — is a requirement (Principle II: each is tested with its exact inputs, amounts to the
cent). All payout-affecting ambiguities were resolved by
the maintainer in the Clarifications section.

### User Story 1 - See a rep's tiered commission for a scenario (Priority: P1)

The user opens the app, picks a seeded scenario from a list, and sees each rep's quarterly
commission. For each rep the page shows the quota, the credited bookings, the attainment, and a
breakdown with one line per amount — each line naming the rule (by FR ID) that produced it.

**Why this priority**: the tiered rate on quota attainment is the core of the compensation plan;
nothing else is meaningful without it.

**Independent Test**: seed a single-rep, full-quarter scenario with no splits, draws or refunds
excluded from the check, and confirm the earned commission and every breakdown line.

**Acceptance Scenarios**:

1. **Given** a rep with a quarterly quota of $100,000.00 (requirement) who starts before the
   quarter and has one deal of $80,000.00 (requirement) counted in the quarter, **When** the
   scenario is selected, **Then** attainment is 80% (requirement) and earned commission is
   $4,000.00 (requirement) — 5% (requirement) of $80,000.00 — on a line citing the tiered-rate
   requirement.
2. **Given** a rep with a quota of $100,000.00 (requirement) and deals totalling $160,000.00
   (requirement) counted in the quarter, **When** the scenario is selected, **Then** earned
   commission is $10,200.00 (requirement): 5% of $100,000.00 = $5,000.00, 8% of $50,000.00 =
   $4,000.00, and 12% of $10,000.00 = $1,200.00, each on its own line citing FR-006.
3. **Given** a rep with a quota of $100,000.00 (requirement) and deals totalling exactly
   $150,000.00 (requirement), **When** the scenario is selected, **Then** earned commission is
   $9,000.00 (requirement): $5,000.00 at 5% and $4,000.00 at 8%, with no 12% line.
4. **Given** a rep with a quota of $100,000.00 (requirement) and one counted deal of $10.10
   (requirement), **When** the scenario is selected, **Then** earned commission is $0.51
   (requirement): 5% of $10.10 is $0.505, rounded half away from zero (FR-018).
5. **Given** the scenario list, **When** the user selects a different scenario, **Then** the page
   shows only that scenario's reps and amounts.

---

### User Story 2 - Only deals that belong to the quarter count (Priority: P1)

Each deal has a close date and a booking date. The user sees which deals were counted for the
quarter and which were excluded, and why.

**Why this priority**: which deals count is an input to every other amount.

**Independent Test**: seed a rep with deals whose close and booking dates straddle the quarter
boundary and confirm which are counted.

**Acceptance Scenarios**:

1. **Given** a quarter of 2026-01-01 to 2026-03-31 (requirement) and a deal of $20,000.00
   (requirement) closed 2026-03-30 and booked 2026-04-02, **When** the scenario is selected,
   **Then** the deal is excluded from the quarter because its booking date is after the quarter's
   last day, and the excluded-deal line states the booking date and cites FR-008.
2. **Given** a quarter of 2026-01-01 to 2026-03-31 (requirement) and a deal of $10,000.00
   (requirement) closed 2025-12-29 and booked 2026-01-05, **When** the scenario is selected,
   **Then** the deal is counted, because its booking date is inside the quarter.
3. **Given** a deal whose close and booking dates both fall inside the quarter, **When** the
   scenario is selected, **Then** it is counted.
4. **Given** a deal whose close and booking dates both fall outside the quarter, **When** the
   scenario is selected, **Then** it is excluded and listed as excluded with the reason.

---

### User Story 3 - Prorated quota for mid-quarter starters (Priority: P2)

A rep who starts partway through the quarter is measured against a prorated quota, and the
breakdown shows how the prorated quota was derived.

**Why this priority**: it changes attainment, and therefore the tier, for every new hire.

**Independent Test**: seed a rep with a start date inside the quarter and confirm the prorated
quota and the resulting commission.

**Acceptance Scenarios**:

1. **Given** a quarter of 2026-01-01 to 2026-03-31 (90 days, requirement), a full quota of
   $90,000.00 (requirement), and a rep starting 2026-02-15, **When** the scenario is selected,
   **Then** the prorated quota is $45,000.00 (requirement): 45 (requirement) of 90 days, on a
   line citing FR-010 that shows both day counts.
2. **Given** that same rep books $50,000.00 (requirement) in the quarter, **When** the scenario is
   selected, **Then** attainment is measured against $45,000.00 and earned commission is
   $2,650.00 (requirement): $2,250.00 at 5% and $400.00 at 8%.
3. **Given** a quarter of 2026-04-01 to 2026-06-30 (91 days, requirement), a full quota of
   $100,000.00 (requirement), a rep starting 2026-05-16 (46 days, requirement) who books
   $60,000.00 (requirement), **When** the scenario is selected, **Then** the prorated quota is
   $50,549.45 (requirement), the 5% line is $2,527.47 (requirement), the 8% line is $756.04
   (requirement), and earned commission is $3,283.51 (requirement) — the sum of the lines shown.
4. **Given** a rep whose start date is on or before the first day of the quarter, **When** the
   scenario is selected, **Then** the quota is not prorated.

---

### User Story 4 - Split deals (Priority: P2)

A deal can be split between several reps. Each rep's breakdown shows their share of the deal and
the commission on that share.

**Why this priority**: shared deals are common and each share moves a rep's attainment.

**Independent Test**: seed a deal split between two reps and confirm each rep's credited share
and commission.

**Acceptance Scenarios**:

1. **Given** a $50,000.00 (requirement) deal split 60% / 40% (requirement) between rep A and
   rep B, **When** the scenario is selected, **Then** rep A's breakdown shows a $30,000.00
   (requirement) share and rep B's a $20,000.00 (requirement) share, each citing the split
   requirement, and each share counts toward that rep's attainment at the share amount only.
2. **Given** rep A with a quota of $100,000.00 (requirement) and $80,000.00 (requirement) of solo
   deals, plus the 60% share of that $50,000.00 split deal, **When** the scenario is selected,
   **Then** rep A's credited bookings are $110,000.00 (requirement), attainment is 110%
   (requirement), and earned commission is $5,800.00 (requirement): $5,000.00 at 5% and $800.00
   at 8%.
3. **Given** a $10.01 (requirement) deal split 50% / 50% (requirement) between rep A (listed
   first) and rep B, **When** the scenario is selected, **Then** rep A's share is $5.01
   (requirement) and rep B's is $5.00 (requirement).
4. **Given** a $100.00 (requirement) deal split 33.335% / 33.335% / 33.33% (requirement) between
   reps A, B and C in that order, **When** the scenario is selected, **Then** the shares are
   $33.34, $33.33 and $33.33 (requirement), summing to $100.00.
5. **Given** a deal whose split percentages sum to anything other than exactly 100%
   (requirement), **When** the scenario is loaded, **Then** the scenario is rejected with a
   message naming the deal and the sum, and no payout is shown for it.

---

### User Story 5 - Monthly draw recovered against commission (Priority: P2)

Each rep receives a monthly draw of $4,000.00 (requirement) that is recovered against earned
commission. The breakdown shows the draws paid, the amount recovered, what is left to pay, and
the recoverable balance carried forward.

**Why this priority**: the draw decides what the rep is actually paid at quarter end.

**Independent Test**: seed reps whose earned commission is above and below the quarter's draw
and confirm the net payout for each.

**Acceptance Scenarios**:

1. **Given** a full-quarter rep who earned $15,000.00 (requirement) in commission and received
   three monthly draws of $4,000.00 (requirement), **When** the scenario is selected, **Then** the
   draw recovered is $12,000.00 (requirement) and the commission still payable is $3,000.00
   (requirement).
2. **Given** a full-quarter rep who earned $4,000.00 (requirement) against $12,000.00
   (requirement) of draws, **When** the scenario is selected, **Then** the $8,000.00
   (requirement) shortfall is carried forward: the draw recovered is $4,000.00 (requirement), the
   commission payable is $0.00 (requirement), and the closing recoverable balance is $8,000.00
   (requirement).
3. **Given** a full-quarter rep with an opening recoverable balance of $8,000.00 (requirement) who
   earns $15,000.00 (requirement) against $12,000.00 (requirement) of draws, **When** the scenario
   is selected, **Then** the draw recovered is $15,000.00 (requirement), the commission payable is
   $0.00 (requirement), and the closing recoverable balance is $5,000.00 (requirement).
4. **Given** a rep who starts mid-quarter, **When** the scenario is selected, **Then** the draw
   paid for the quarter is prorated per FR-014.
5. **Given** a quarter of 2026-01-01 to 2026-03-31 (requirement) and a rep starting 2026-02-15 who
   earns $2,650.00 (requirement), **When** the scenario is selected, **Then** the February draw is
   $2,000.00 (requirement; 14 of 28 days), the March draw is $4,000.00 (requirement), the draw
   recovered is $2,650.00 (requirement), the commission payable is $0.00 (requirement), and the
   closing recoverable balance is $3,350.00 (requirement).
6. **Given** a quarter of 2026-01-01 to 2026-03-31 (requirement) and a rep starting 2026-01-20,
   **When** the scenario is selected, **Then** the January draw is $1,548.39 (requirement; 12 of
   31 days, rounded per FR-018) and the total draw paid is $9,548.39 (requirement).

---

### User Story 6 - Clawback on refunded deals (Priority: P3)

When a deal is refunded, the commission on that deal is clawed back, shown as its own line.

**Why this priority**: refunds are the exception, but a wrong clawback is a wrong paycheck.

**Independent Test**: seed a rep with two deals, one refunded, and confirm the clawback line and
the net result.

**Acceptance Scenarios**:

1. **Given** a rep with a quota of $100,000.00 (requirement) and two counted deals of $60,000.00
   each (requirement), deal A (the earlier) refunded in full, **When** the scenario is selected,
   **Then** commission before refunds is $6,600.00 (requirement), the clawback line citing FR-016
   is $3,600.00 (requirement), and earned commission after the clawback is $3,000.00
   (requirement) — the same as if deal A had never been booked. (Deal A's refund date is inside
   the quarter.)
2. **Given** a rep whose Q1 2026 had a quota of $100,000.00 (requirement) and two deals of
   $60,000.00 (requirement) each, deal A refunded in full on 2026-04-15, and whose Q2 2026
   (2026-04-01 to 2026-06-30) has a quota of $100,000.00 (requirement), $40,000.00 (requirement)
   of counted deals, three draws of $4,000.00 (requirement) and an opening recoverable balance of
   $0.00, **When** the Q2 scenario is selected, **Then** commission before refunds is $2,000.00
   (requirement), the clawback line citing FR-016 is $3,600.00 (requirement), earned commission
   is −$1,600.00 (requirement), the draw recovered is $0.00, the commission payable is $0.00, and
   the closing recoverable balance is $13,600.00 (requirement).
3. **Given** a deal booked inside the quarter whose refund date is after the quarter's last day,
   **When** the scenario is selected, **Then** the deal counts normally and no clawback appears in
   this quarter's statement.
4. **Given** a rep with a quota of $100,000.00 (requirement) and two counted deals of $60,000.00
   (requirement) each, $30,000.00 (requirement) of deal A refunded inside the quarter, **When** the
   scenario is selected, **Then** commission before refunds is $6,600.00 (requirement), the
   clawback is $2,100.00 (requirement), and earned commission is $4,500.00 (requirement).
5. **Given** a rep whose Q1 2026 had a quota of $100,000.00 (requirement) and two deals of
   $60,000.00 (requirement) each, with $20,000.00 (requirement) of deal A refunded on 2026-02-20
   and another $20,000.00 (requirement) on 2026-05-05, **When** the Q1 scenario is selected,
   **Then** its clawback is $1,600.00 (requirement); **and when** the Q2 scenario is selected, its
   clawback for deal A is $1,000.00 (requirement).
6. **Given** a rep with a quota of $100,000.00 (requirement) and two counted deals of $60,000.00
   (requirement) each, deal A refunded in full on 2026-02-20 and deal B refunded in full on
   2026-03-10, **When** the scenario is selected, **Then** the clawback for deal A is $3,600.00
   (requirement), the clawback for deal B is $3,000.00 (requirement), and earned commission is
   $0.00 (requirement).
7. **Given** reps A (listed first) and B, each with a quota of $100,000.00 (requirement), sharing
   a $10,000.20 (requirement) deal split 50% / 50% (requirement), with $100.01 (requirement)
   refunded inside the quarter, **When** the scenario is selected, **Then** each rep's
   commission before refunds is $250.01 (requirement), the reduced deal of $9,900.19 is re-split
   as $4,950.10 to A and $4,950.09 to B (requirement), A's clawback is $2.50 (requirement) and
   B's clawback is $2.51 (requirement).
8. **Given** a Q2 2026 scenario for rep A carrying A's Q1 2026 data — quota $100,000.00
   (requirement), a solo deal D1 of $60,000.00 (requirement) refunded in full on 2026-04-20, and
   a $40,000.00 (requirement) deal D2 split 50% / 50% (requirement) with rep Y, who appears only
   in the Q1 data — **When** the Q2 scenario is selected, **Then** rep A's clawback for D1 is
   $3,000.00 (requirement) and rep Y has no statement.
9. **Given** rep C with a quota of $100,000.00 (requirement), $10.09 (requirement) of solo deals,
   and a 10% share of a $0.06 (requirement) deal split 45% / 45% / 10% (requirement) between reps
   A, B and C in that order, with $0.01 (requirement) of that deal refunded inside the quarter,
   **When** the scenario is selected, **Then** C's commission before refunds is $0.50
   (requirement), C's re-split share is $0.01 (requirement), C's clawback line is −$0.01
   (requirement), and C's earned commission is $0.51 (requirement).

---

### Edge Cases

- A deal amount, quota or refund of zero or less; any monetary input (deal amount, quota, refund,
  opening recoverable balance) that is not a whole number of cents; a negative opening recoverable
  balance; or a split percentage that is not greater than 0% and at most 100% (requirement): the
  scenario is rejected with a message; no payouts are shown for it (FR-004).
- A deal in the scenario's own deal list (counted or not) credited to a rep who is not on the
  roster: rejected (FR-004). Booking-
  quarter data supplied for clawbacks may name reps not on the roster (with their start dates);
  they get no statement (FR-016).
- A scenario quarter that is not exactly three (requirement) whole calendar months starting on the
  first day of a month: rejected (FR-014 counts draws per calendar month).
- A refund dated before the deal's booking date: rejected.
- A rep whose start date is after the quarter's last day: the scenario is rejected, naming the rep
  (FR-011).
- A deal booked before the start date of a rep credited on it, whether or not the deal counts this
  quarter: the scenario is rejected, naming the deal and the rep (FR-011).
- Amounts that do not divide to whole cents (e.g. 5% of $10.10 (requirement) is $0.505
  (requirement)): rounded per FR-018 to $0.51 (US1 AS4).
- A partial refund: clawed back per FR-017. A refund of zero or less, or refunds on one deal
  totalling more than the deal amount, are rejected.
- A clawback larger than the commission earned in the quarter: earned commission goes negative
  and the magnitude is added to the closing recoverable balance (FR-015, FR-016).

## Requirements *(mandatory)*

### Functional Requirements

**Scenarios and display**

- **FR-001**: The system MUST offer a list of seeded scenarios and let the user select one.
- **FR-002**: For the selected scenario, the system MUST show every rep in it with their quota,
  credited bookings, attainment, earned commission, draws paid, recovery, clawbacks, commission
  payable and closing recoverable balance. Attainment is shown as a percentage to two decimal
  places (requirement), rounded half away from zero; it is display only and never used to compute
  an amount.
- **FR-003**: Each rep's result MUST include a line-by-line breakdown in which every amount is on
  its own line and each line cites the FR ID of the rule that produced it — including the subtotals
  "Credited bookings" (FR-008), "Clawbacks" (FR-016, the sum of the clawback lines, $0.00 when
  there are none) and "Draws paid" (FR-014) and, after each refund on a split deal, the rep's
  re-split share (FR-017), as Appendix A shows. Each credit or excluded-deal line shows the deal's
  close date for information (FR-008).
- **FR-004**: A scenario that violates a validation rule (FR-011, FR-013, Edge Cases) MUST be shown
  as rejected with the reasons, and MUST NOT show any payout. Every monetary input MUST be a whole
  number of cents; deal amounts, quotas and refunds MUST be greater than zero; an opening
  recoverable balance MUST be zero or more; every split percentage MUST be greater than 0%
  (requirement) and at most 100% (requirement). A scenario MUST also be rejected, naming the rep,
  deal or quarter involved, when: its roster is empty; two roster reps share a repId; two deals in
  one quarter's deal list share a dealId; a rep appears more than once on one deal; a prorated quota rounds to $0.00 (requirement); any deal in the
  scenario's own deal list (counted or not) is credited to a rep not on the roster; a deal
  refunded in the quarter was booked in an earlier quarter whose data is missing or incomplete
  (detectably incomplete means: the booking quarter is absent; a roster rep credited on the deal
  has no entry in its `reps` (quota and start date); or a split partner not on the roster, on any
  deal in the booking quarter, has no start date in its `partners` — needed because FR-011's
  start-date check covers every booking-quarter deal), is not three whole calendar months, overlaps the scenario's quarter, or
  contains a deal booked outside its own dates; or a rep's start date in booking-quarter data
  differs from the roster's; or an earlier-quarter deal listed both in the scenario's own deal
  list and in booking-quarter data differs between the two. **Precondition (not detectable by the
  engine):** booking-quarter data includes every deal each credited roster rep booked in that
  quarter and every refund on those deals dated before the refund being sized, whatever quarter it
  falls in; the shipped seed files meet it because they must equal Appendix A.

**Quota and rates**

- **FR-005**: Each rep MUST have a quarterly quota in USD, greater than zero.
- **FR-006**: Commission MUST be tiered on quota attainment at 5% (requirement) up to 100%
  (requirement) of quota, 8% (requirement) from 100% to 150% (requirement), and 12%
  (requirement) above 150%, applied marginally: each slice of credited bookings earns its own
  band's rate, and each band with a non-zero slice is its own breakdown line.
- **FR-007**: Band edges are at exactly 100% (requirement) and 150% (requirement) of quota: the 5%
  band covers credited bookings up to and including 100% of quota, the 8% band the part above 100%
  up to and including 150%, and the 12% band the part above 150% (rates and edges as tagged in
  FR-006). (Under marginal tiers the edge assignment cannot
  change an amount; it fixes which line a slice appears on.)
- **FR-008**: A deal MUST count toward the quarter in which its booking date falls (first and last
  day inclusive); its close date does not affect crediting and is shown for information only.
  Deals booked outside the quarter are excluded and listed with the reason.
- **FR-009**: Attainment MUST be the rep's credited bookings divided by the rep's (prorated, where
  FR-010 applies) quota.

**Proration**

- **FR-010**: A rep whose start date is after the first day of the quarter MUST have their quota
  prorated by calendar days: full quota × (days from the start date to the quarter's last day,
  both inclusive) ÷ (days in the quarter, both ends inclusive).
- **FR-011**: A scenario in which any deal — counted this quarter or not, including booking-quarter
  data supplied for clawbacks — is credited to a rep whose start date is after that deal's booking
  date MUST be rejected, naming the deal and the rep. A scenario listing a rep whose
  start date is after the quarter's last day MUST be rejected, naming the rep.

**Splits**

- **FR-012**: A deal MAY be split between reps; each rep's credited share is the deal amount times
  their split percentage. The share, and only the share, counts toward that rep's credited
  bookings — both for attainment and for commission — so credit across all reps sums to the deal
  amount. Shares are allocated to the cent by largest remainder: each share is first rounded down
  to the cent, then the leftover cents are given one at a time to the shares with the largest
  fractional remainders, ties going to the rep listed first on the deal. After a refund, the
  same allocation re-splits the reduced deal amount across the deal's reps (FR-017). This is the one exception to
  FR-018's half-away-from-zero rounding.
- **FR-013**: A deal's split percentages MUST sum to exactly 100% (requirement); otherwise the
  scenario is rejected.

**Draw**

- **FR-014**: Each rep MUST receive a draw of $4,000.00 (requirement) for each calendar month of
  the quarter in which they are employed. In the month containing a mid-quarter start date the
  draw is $4,000.00 (requirement) × (days from the start date to that month's last day, both inclusive) ÷ (days
  in that month); months before the start month carry no draw. Each month's draw is its own
  breakdown line.
- **FR-015**: Each rep has an opening recoverable balance (default $0.00, requirement) of draw not yet
  recovered from earlier quarters. The recoverable total is that balance plus the draws paid this
  quarter. Earned commission is commission before refunds minus clawbacks, and may be negative.
  When earned commission is zero or more, the amount recovered is the lesser of earned commission
  and the recoverable total, commission payable is earned commission minus the amount recovered,
  and the closing recoverable balance is the recoverable total minus the amount recovered. When
  earned commission is negative, the amount recovered and commission payable are $0.00 and the
  closing recoverable balance is the recoverable total plus the negative amount's magnitude.
  Commission payable is never negative. Each of these is its own breakdown line.

**Refunds**

- **FR-016**: When a deal is refunded, the commission on that deal MUST be clawed back, one line
  per refund, in the statement of the quarter containing the refund date, and sized per FR-017.
  A scenario includes, for any deal refunded in its quarter but booked in an earlier one, that
  booking quarter's data for the affected reps (quarter dates, quota, start date, deals, and every
  earlier refund on those deals, whatever quarter it is dated in). That data may name split partners who are not on this quarter's roster; they
  are listed with their start dates and receive no statement. A refund dated after the scenario's quarter ends has no effect on that
  quarter's statement.
- **FR-017**: A deal MAY have several refunds, each partial or full, as long as they total no
  more than the deal amount. For each rep, all refunds on deals booked in the same quarter are
  applied in refund-date order across deals; refunds on the same date are ordered by their deal's
  position in that quarter's deal list, then by their position within the deal. (The order can
  move an amount between two clawback lines but never changes their total.) Each refund's clawback is
  that booking quarter's commission computed with every earlier refund applied, minus the same
  quarter computed with this refund also applied; a refunded amount no longer counts toward
  attainment. On a split deal, after each refund the reduced deal amount (the deal amount minus
  all refunds applied so far) is re-split across its reps by FR-012's largest-remainder
  allocation, and each rep's clawback is computed from their re-split credit. A full refund is the
  case where the refunds total the deal amount. A clawback is exactly the recomputed difference
  and may be negative, when a re-split raises a rep's credit.

**Money**

- **FR-018**: Every computed monetary amount — prorated quota, the 150% (requirement) band edge,
  prorated draw, each tier line, clawback, recovery — MUST be rounded to the cent when it is
  produced, half away from zero, and later steps MUST use the rounded value. Every total MUST
  equal the sum of the lines shown. Split shares are the exception and follow FR-012.
- **FR-019**: All amounts MUST be in USD. Tax, currency conversion and multi-year contract handling
  are out of scope.

**Application**

- **FR-020**: The application MUST run locally from a single command, with no external services,
  no authentication, and no database beyond in-memory data or a local file.
- **FR-021**: The interface MUST be operable by keyboard alone and by screen reader, with the
  breakdown presented as a table with header cells.

### Key Entities

- **Scenario**: a named, seeded quarter (a first and last date), its roster of reps, its deals,
  and any booking-quarter data needed to size clawbacks for refunds dated in the quarter.
- **Rep**: name, quarterly quota (USD), start date, and opening recoverable draw balance (USD,
  default $0.00; FR-015).
- **Deal**: identifier, amount (USD), close date, booking date, one or more split credits (rep and
  percentage, in listed order), and zero or more refunds (amount and date).
- **Commission result**: per rep — prorated quota, credited bookings, attainment, and the ordered
  breakdown lines, each with amount, description and the citing FR ID.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For 100% (requirement) of the seeded scenarios, every rep's statement matches
  Appendix A line for line, to the cent, and the rejected scenario lists exactly the errors
  Appendix A states.
- **SC-002**: 100% (requirement) of breakdown lines cite the FR that produced them, and every cited
  FR exists in this spec.
- **SC-003**: Each of the eight compensation rules in the brief (requirement: all eight) is
  exercised by at least one seeded scenario.
- **SC-004**: From a fresh clone, the application starts with one command (requirement) and needs
  no network service at run time.
- **SC-005**: A keyboard-only user can select any scenario and reach every rep's breakdown without
  a pointing device (requirement).

## Assumptions

- Each scenario states its own quarter as a first and last date, so no fiscal calendar is assumed.
- Quota is set per quarter, not derived from an annual figure.
- A rep who starts on or before the quarter's first day has a full quarter.
- Scenarios are read-only seeded data; editing or creating scenarios is out of scope.
- Only the rules in the brief apply: no accelerators, caps, SPIFs, or rep terminations mid-quarter.

## Appendix A: Seeded scenarios and expected statements

The seeded scenarios the application ships with, and the full statement expected for every rep in
each. **Every amount in this appendix is a requirement** (SC-001; Principle II): each line is
tested to the cent. The amounts were computed from FR-005–FR-018 by a reference calculation
written independently of the application, and cross-checked against every amount stated in the
acceptance scenarios above (all match). Each scenario lists the acceptance examples it carries.
Reps whose start date is before the quarter have a full quarter; the opening recoverable balance
is $0.00 unless shown.

### A.1 `tiers`: Tiered rates

Quarter 2026-01-01 to 2026-03-31 (90 days). Carries US1 AS1–AS4.

| Rep | Quota | Start date | Opening balance |
|---|---|---|---|
| Avery (`avery`) | $100,000.00 | 2025-06-01 | $0.00 |
| Blake (`blake`) | $100,000.00 | 2025-06-01 | $0.00 |
| Casey (`casey`) | $100,000.00 | 2025-06-01 | $0.00 |
| Devon (`devon`) | $100,000.00 | 2025-06-01 | $0.00 |

Deals:

| Deal | Amount | Close date | Booking date | Credit | Refunds |
|---|---|---|---|---|---|
| T-1 | $80,000.00 | 2026-02-10 | 2026-02-12 | avery 100% | — |
| T-2 | $100,000.00 | 2026-01-20 | 2026-01-22 | blake 100% | — |
| T-3 | $60,000.00 | 2026-03-02 | 2026-03-04 | blake 100% | — |
| T-4 | $150,000.00 | 2026-02-26 | 2026-03-02 | casey 100% | — |
| T-5 | $10.10 | 2026-01-15 | 2026-01-16 | devon 100% | — |

**Avery**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| T-1 booked 2026-02-12 (closed 2026-02-10) | $80,000.00 | FR-008 |
| Credited bookings | $80,000.00 | FR-008 |
| 5% of $80,000.00 | $4,000.00 | FR-006 |
| Commission before refunds | $4,000.00 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $4,000.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $4,000.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $8,000.00 | FR-015 |

**Blake**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| T-2 booked 2026-01-22 (closed 2026-01-20) | $100,000.00 | FR-008 |
| T-3 booked 2026-03-04 (closed 2026-03-02) | $60,000.00 | FR-008 |
| Credited bookings | $160,000.00 | FR-008 |
| 5% of $100,000.00 | $5,000.00 | FR-006 |
| 8% of $50,000.00 | $4,000.00 | FR-006 |
| 12% of $10,000.00 | $1,200.00 | FR-006 |
| Commission before refunds | $10,200.00 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $10,200.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $10,200.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $1,800.00 | FR-015 |

**Casey**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| T-4 booked 2026-03-02 (closed 2026-02-26) | $150,000.00 | FR-008 |
| Credited bookings | $150,000.00 | FR-008 |
| 5% of $100,000.00 | $5,000.00 | FR-006 |
| 8% of $50,000.00 | $4,000.00 | FR-006 |
| Commission before refunds | $9,000.00 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $9,000.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $9,000.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $3,000.00 | FR-015 |

**Devon**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| T-5 booked 2026-01-16 (closed 2026-01-15) | $10.10 | FR-008 |
| Credited bookings | $10.10 | FR-008 |
| 5% of $10.10 | $0.51 | FR-006 |
| Commission before refunds | $0.51 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $0.51 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $0.51 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $11,999.49 | FR-015 |

### A.2 `booking-dates`: Close date vs booking date

Quarter 2026-01-01 to 2026-03-31 (90 days). Carries US2 AS1–AS4.

| Rep | Quota | Start date | Opening balance |
|---|---|---|---|
| Emery (`emery`) | $100,000.00 | 2025-01-01 | $0.00 |

Deals:

| Deal | Amount | Close date | Booking date | Credit | Refunds |
|---|---|---|---|---|---|
| B-1 | $20,000.00 | 2026-03-30 | 2026-04-02 | emery 100% | — |
| B-2 | $10,000.00 | 2025-12-29 | 2026-01-05 | emery 100% | — |
| B-3 | $30,000.00 | 2026-02-03 | 2026-02-04 | emery 100% | — |
| B-4 | $15,000.00 | 2025-12-10 | 2025-12-15 | emery 100% | — |

**Emery**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| B-1 booked 2026-04-02 (closed 2026-03-30): excluded, booked outside the quarter | $0.00 | FR-008 |
| B-2 booked 2026-01-05 (closed 2025-12-29) | $10,000.00 | FR-008 |
| B-3 booked 2026-02-04 (closed 2026-02-03) | $30,000.00 | FR-008 |
| B-4 booked 2025-12-15 (closed 2025-12-10): excluded, booked outside the quarter | $0.00 | FR-008 |
| Credited bookings | $40,000.00 | FR-008 |
| 5% of $40,000.00 | $2,000.00 | FR-006 |
| Commission before refunds | $2,000.00 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $2,000.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $2,000.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $10,000.00 | FR-015 |

### A.3 `proration`: Mid-quarter starters

Quarter 2026-01-01 to 2026-03-31 (90 days). Carries US3 AS1–AS2, US5 AS4–AS6.

| Rep | Quota | Start date | Opening balance |
|---|---|---|---|
| Finley (`finley`) | $90,000.00 | 2026-02-15 | $0.00 |
| Gray (`gray`) | $90,000.00 | 2026-01-20 | $0.00 |

Deals:

| Deal | Amount | Close date | Booking date | Credit | Refunds |
|---|---|---|---|---|---|
| P-1 | $50,000.00 | 2026-03-09 | 2026-03-10 | finley 100% | — |
| P-2 | $30,000.00 | 2026-01-30 | 2026-02-02 | gray 100% | — |

**Finley**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $90,000.00 | FR-005 |
| Prorated quota (45 of 90 days) | $45,000.00 | FR-010 |
| P-1 booked 2026-03-10 (closed 2026-03-09) | $50,000.00 | FR-008 |
| Credited bookings | $50,000.00 | FR-008 |
| 5% of $45,000.00 | $2,250.00 | FR-006 |
| 8% of $5,000.00 | $400.00 | FR-006 |
| Commission before refunds | $2,650.00 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $2,650.00 | FR-015 |
| Draw, January 2026 | $0.00 | FR-014 |
| Draw, February 2026 | $2,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $6,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $2,650.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $3,350.00 | FR-015 |

**Gray**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $90,000.00 | FR-005 |
| Prorated quota (71 of 90 days) | $71,000.00 | FR-010 |
| P-2 booked 2026-02-02 (closed 2026-01-30) | $30,000.00 | FR-008 |
| Credited bookings | $30,000.00 | FR-008 |
| 5% of $30,000.00 | $1,500.00 | FR-006 |
| Commission before refunds | $1,500.00 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $1,500.00 | FR-015 |
| Draw, January 2026 | $1,548.39 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $9,548.39 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $1,500.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $8,048.39 | FR-015 |

### A.4 `proration-q2`: Mid-quarter starter, rounded proration

Quarter 2026-04-01 to 2026-06-30 (91 days). Carries US3 AS3.

| Rep | Quota | Start date | Opening balance |
|---|---|---|---|
| Harper (`harper`) | $100,000.00 | 2026-05-16 | $0.00 |

Deals:

| Deal | Amount | Close date | Booking date | Credit | Refunds |
|---|---|---|---|---|---|
| P-3 | $60,000.00 | 2026-06-01 | 2026-06-03 | harper 100% | — |

**Harper**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| Prorated quota (46 of 91 days) | $50,549.45 | FR-010 |
| P-3 booked 2026-06-03 (closed 2026-06-01) | $60,000.00 | FR-008 |
| Credited bookings | $60,000.00 | FR-008 |
| 5% of $50,549.45 | $2,527.47 | FR-006 |
| 8% of $9,450.55 | $756.04 | FR-006 |
| Commission before refunds | $3,283.51 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $3,283.51 | FR-015 |
| Draw, April 2026 | $0.00 | FR-014 |
| Draw, May 2026 | $2,064.52 | FR-014 |
| Draw, June 2026 | $4,000.00 | FR-014 |
| Draws paid | $6,064.52 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $3,283.51 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $2,781.01 | FR-015 |

### A.5 `splits`: Split deals

Quarter 2026-01-01 to 2026-03-31 (90 days). Carries US4 AS1–AS2.

| Rep | Quota | Start date | Opening balance |
|---|---|---|---|
| Indigo (`indigo`) | $100,000.00 | 2025-06-01 | $0.00 |
| Jules (`jules`) | $100,000.00 | 2025-06-01 | $0.00 |

Deals:

| Deal | Amount | Close date | Booking date | Credit | Refunds |
|---|---|---|---|---|---|
| S-1 | $80,000.00 | 2026-01-12 | 2026-01-13 | indigo 100% | — |
| S-2 | $50,000.00 | 2026-02-17 | 2026-02-18 | indigo 60%, jules 40% | — |

**Indigo**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| S-1 booked 2026-01-13 (closed 2026-01-12) | $80,000.00 | FR-008 |
| S-2 booked 2026-02-18 (closed 2026-02-17): 60% share of $50,000.00 | $30,000.00 | FR-012 |
| Credited bookings | $110,000.00 | FR-008 |
| 5% of $100,000.00 | $5,000.00 | FR-006 |
| 8% of $10,000.00 | $800.00 | FR-006 |
| Commission before refunds | $5,800.00 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $5,800.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $5,800.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $6,200.00 | FR-015 |

**Jules**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| S-2 booked 2026-02-18 (closed 2026-02-17): 40% share of $50,000.00 | $20,000.00 | FR-012 |
| Credited bookings | $20,000.00 | FR-008 |
| 5% of $20,000.00 | $1,000.00 | FR-006 |
| Commission before refunds | $1,000.00 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $1,000.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $1,000.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $11,000.00 | FR-015 |

### A.6 `split-rounding`: Split shares to the cent

Quarter 2026-01-01 to 2026-03-31 (90 days). Carries US4 AS3–AS4.

| Rep | Quota | Start date | Opening balance |
|---|---|---|---|
| Kai (`kai`) | $100,000.00 | 2025-06-01 | $0.00 |
| Lee (`lee`) | $100,000.00 | 2025-06-01 | $0.00 |
| Mo (`mo`) | $100,000.00 | 2025-06-01 | $0.00 |
| Nat (`nat`) | $100,000.00 | 2025-06-01 | $0.00 |
| Oli (`oli`) | $100,000.00 | 2025-06-01 | $0.00 |

Deals:

| Deal | Amount | Close date | Booking date | Credit | Refunds |
|---|---|---|---|---|---|
| S-3 | $10.01 | 2026-02-02 | 2026-02-03 | kai 50%, lee 50% | — |
| S-4 | $100.00 | 2026-03-05 | 2026-03-06 | mo 33.335%, nat 33.335%, oli 33.33% | — |

**Kai**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| S-3 booked 2026-02-03 (closed 2026-02-02): 50% share of $10.01 | $5.01 | FR-012 |
| Credited bookings | $5.01 | FR-008 |
| 5% of $5.01 | $0.25 | FR-006 |
| Commission before refunds | $0.25 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $0.25 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $0.25 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $11,999.75 | FR-015 |

**Lee**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| S-3 booked 2026-02-03 (closed 2026-02-02): 50% share of $10.01 | $5.00 | FR-012 |
| Credited bookings | $5.00 | FR-008 |
| 5% of $5.00 | $0.25 | FR-006 |
| Commission before refunds | $0.25 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $0.25 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $0.25 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $11,999.75 | FR-015 |

**Mo**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| S-4 booked 2026-03-06 (closed 2026-03-05): 33.335% share of $100.00 | $33.34 | FR-012 |
| Credited bookings | $33.34 | FR-008 |
| 5% of $33.34 | $1.67 | FR-006 |
| Commission before refunds | $1.67 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $1.67 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $1.67 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $11,998.33 | FR-015 |

**Nat**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| S-4 booked 2026-03-06 (closed 2026-03-05): 33.335% share of $100.00 | $33.33 | FR-012 |
| Credited bookings | $33.33 | FR-008 |
| 5% of $33.33 | $1.67 | FR-006 |
| Commission before refunds | $1.67 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $1.67 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $1.67 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $11,998.33 | FR-015 |

**Oli**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| S-4 booked 2026-03-06 (closed 2026-03-05): 33.33% share of $100.00 | $33.33 | FR-012 |
| Credited bookings | $33.33 | FR-008 |
| 5% of $33.33 | $1.67 | FR-006 |
| Commission before refunds | $1.67 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $1.67 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $1.67 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $11,998.33 | FR-015 |

### A.7 `draw`: Draw recovery

Quarter 2026-01-01 to 2026-03-31 (90 days). Carries US5 AS1–AS3.

| Rep | Quota | Start date | Opening balance |
|---|---|---|---|
| Parker (`parker`) | $100,000.00 | 2025-06-01 | $0.00 |
| Quinn (`quinn`) | $100,000.00 | 2025-06-01 | $0.00 |
| Reese (`reese`) | $100,000.00 | 2025-06-01 | $8,000.00 |

Deals:

| Deal | Amount | Close date | Booking date | Credit | Refunds |
|---|---|---|---|---|---|
| W-1 | $200,000.00 | 2026-02-20 | 2026-02-23 | parker 100% | — |
| W-2 | $80,000.00 | 2026-03-11 | 2026-03-12 | quinn 100% | — |
| W-3 | $200,000.00 | 2026-01-26 | 2026-01-27 | reese 100% | — |

**Parker**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| W-1 booked 2026-02-23 (closed 2026-02-20) | $200,000.00 | FR-008 |
| Credited bookings | $200,000.00 | FR-008 |
| 5% of $100,000.00 | $5,000.00 | FR-006 |
| 8% of $50,000.00 | $4,000.00 | FR-006 |
| 12% of $50,000.00 | $6,000.00 | FR-006 |
| Commission before refunds | $15,000.00 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $15,000.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $12,000.00 | FR-015 |
| Commission payable | $3,000.00 | FR-015 |
| Closing recoverable balance | $0.00 | FR-015 |

**Quinn**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| W-2 booked 2026-03-12 (closed 2026-03-11) | $80,000.00 | FR-008 |
| Credited bookings | $80,000.00 | FR-008 |
| 5% of $80,000.00 | $4,000.00 | FR-006 |
| Commission before refunds | $4,000.00 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $4,000.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $4,000.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $8,000.00 | FR-015 |

**Reese**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| W-3 booked 2026-01-27 (closed 2026-01-26) | $200,000.00 | FR-008 |
| Credited bookings | $200,000.00 | FR-008 |
| 5% of $100,000.00 | $5,000.00 | FR-006 |
| 8% of $50,000.00 | $4,000.00 | FR-006 |
| 12% of $50,000.00 | $6,000.00 | FR-006 |
| Commission before refunds | $15,000.00 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $15,000.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $8,000.00 | FR-015 |
| Draw recovered | $15,000.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $5,000.00 | FR-015 |

### A.8 `refunds`: Refunds inside the quarter

Quarter 2026-01-01 to 2026-03-31 (90 days). Carries US6 AS1, AS3, AS4, AS5 (Q1), AS6.

| Rep | Quota | Start date | Opening balance |
|---|---|---|---|
| Sage (`sage`) | $100,000.00 | 2025-06-01 | $0.00 |
| Tatum (`tatum`) | $100,000.00 | 2025-06-01 | $0.00 |
| Uma (`uma`) | $100,000.00 | 2025-06-01 | $0.00 |
| Val (`val`) | $100,000.00 | 2025-06-01 | $0.00 |
| Wren (`wren`) | $100,000.00 | 2025-06-01 | $0.00 |

Deals:

| Deal | Amount | Close date | Booking date | Credit | Refunds |
|---|---|---|---|---|---|
| R-1 | $60,000.00 | 2026-01-09 | 2026-01-10 | sage 100% | $60,000.00 on 2026-02-15 |
| R-2 | $60,000.00 | 2026-02-09 | 2026-02-10 | sage 100% | — |
| R-3 | $60,000.00 | 2026-01-09 | 2026-01-10 | tatum 100% | $20,000.00 on 2026-04-20 |
| R-4 | $60,000.00 | 2026-02-09 | 2026-02-10 | tatum 100% | — |
| R-5 | $60,000.00 | 2026-01-09 | 2026-01-10 | uma 100% | $30,000.00 on 2026-03-01 |
| R-6 | $60,000.00 | 2026-02-09 | 2026-02-10 | uma 100% | — |
| R-7 | $60,000.00 | 2026-01-09 | 2026-01-10 | val 100% | $20,000.00 on 2026-02-20, $20,000.00 on 2026-05-05 |
| R-8 | $60,000.00 | 2026-02-09 | 2026-02-10 | val 100% | — |
| R-9 | $60,000.00 | 2026-01-09 | 2026-01-10 | wren 100% | $60,000.00 on 2026-02-20 |
| R-10 | $60,000.00 | 2026-01-12 | 2026-01-13 | wren 100% | $60,000.00 on 2026-03-10 |

**Sage**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| R-1 booked 2026-01-10 (closed 2026-01-09) | $60,000.00 | FR-008 |
| R-2 booked 2026-02-10 (closed 2026-02-09) | $60,000.00 | FR-008 |
| Credited bookings | $120,000.00 | FR-008 |
| 5% of $100,000.00 | $5,000.00 | FR-006 |
| 8% of $20,000.00 | $1,600.00 | FR-006 |
| Commission before refunds | $6,600.00 | FR-006 |
| Clawback: R-1 refund $60,000.00 on 2026-02-15 | $3,600.00 | FR-016 |
| Clawbacks | $3,600.00 | FR-016 |
| Earned commission | $3,000.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $3,000.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $9,000.00 | FR-015 |

**Tatum**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| R-3 booked 2026-01-10 (closed 2026-01-09) | $60,000.00 | FR-008 |
| R-4 booked 2026-02-10 (closed 2026-02-09) | $60,000.00 | FR-008 |
| Credited bookings | $120,000.00 | FR-008 |
| 5% of $100,000.00 | $5,000.00 | FR-006 |
| 8% of $20,000.00 | $1,600.00 | FR-006 |
| Commission before refunds | $6,600.00 | FR-006 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $6,600.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $6,600.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $5,400.00 | FR-015 |

**Uma**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| R-5 booked 2026-01-10 (closed 2026-01-09) | $60,000.00 | FR-008 |
| R-6 booked 2026-02-10 (closed 2026-02-09) | $60,000.00 | FR-008 |
| Credited bookings | $120,000.00 | FR-008 |
| 5% of $100,000.00 | $5,000.00 | FR-006 |
| 8% of $20,000.00 | $1,600.00 | FR-006 |
| Commission before refunds | $6,600.00 | FR-006 |
| Clawback: R-5 refund $30,000.00 on 2026-03-01 | $2,100.00 | FR-016 |
| Clawbacks | $2,100.00 | FR-016 |
| Earned commission | $4,500.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $4,500.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $7,500.00 | FR-015 |

**Val**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| R-7 booked 2026-01-10 (closed 2026-01-09) | $60,000.00 | FR-008 |
| R-8 booked 2026-02-10 (closed 2026-02-09) | $60,000.00 | FR-008 |
| Credited bookings | $120,000.00 | FR-008 |
| 5% of $100,000.00 | $5,000.00 | FR-006 |
| 8% of $20,000.00 | $1,600.00 | FR-006 |
| Commission before refunds | $6,600.00 | FR-006 |
| Clawback: R-7 refund $20,000.00 on 2026-02-20 | $1,600.00 | FR-016 |
| Clawbacks | $1,600.00 | FR-016 |
| Earned commission | $5,000.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $5,000.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $7,000.00 | FR-015 |

**Wren**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| R-9 booked 2026-01-10 (closed 2026-01-09) | $60,000.00 | FR-008 |
| R-10 booked 2026-01-13 (closed 2026-01-12) | $60,000.00 | FR-008 |
| Credited bookings | $120,000.00 | FR-008 |
| 5% of $100,000.00 | $5,000.00 | FR-006 |
| 8% of $20,000.00 | $1,600.00 | FR-006 |
| Commission before refunds | $6,600.00 | FR-006 |
| Clawback: R-9 refund $60,000.00 on 2026-02-20 | $3,600.00 | FR-016 |
| Clawback: R-10 refund $60,000.00 on 2026-03-10 | $3,000.00 | FR-016 |
| Clawbacks | $6,600.00 | FR-016 |
| Earned commission | $0.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $0.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $12,000.00 | FR-015 |

### A.9 `refund-splits`: Refunds on split deals

Quarter 2026-01-01 to 2026-03-31 (90 days). Carries US6 AS7, AS9.

| Rep | Quota | Start date | Opening balance |
|---|---|---|---|
| Xan (`xan`) | $100,000.00 | 2025-06-01 | $0.00 |
| Yael (`yael`) | $100,000.00 | 2025-06-01 | $0.00 |
| Ari (`ari`) | $100,000.00 | 2025-06-01 | $0.00 |
| Bo (`bo`) | $100,000.00 | 2025-06-01 | $0.00 |
| Cy (`cy`) | $100,000.00 | 2025-06-01 | $0.00 |

Deals:

| Deal | Amount | Close date | Booking date | Credit | Refunds |
|---|---|---|---|---|---|
| X-1 | $10,000.20 | 2026-01-06 | 2026-01-07 | xan 50%, yael 50% | $100.01 on 2026-02-10 |
| X-2 | $10.09 | 2026-01-06 | 2026-01-07 | cy 100% | — |
| X-3 | $0.06 | 2026-01-08 | 2026-01-09 | ari 45%, bo 45%, cy 10% | $0.01 on 2026-02-11 |

**Xan**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| X-1 booked 2026-01-07 (closed 2026-01-06): 50% share of $10,000.20 | $5,000.10 | FR-012 |
| Credited bookings | $5,000.10 | FR-008 |
| 5% of $5,000.10 | $250.01 | FR-006 |
| Commission before refunds | $250.01 | FR-006 |
| X-1 re-split after refund on 2026-02-10: share of $9,900.19 | $4,950.10 | FR-017 |
| Clawback: X-1 refund $100.01 on 2026-02-10 | $2.50 | FR-016 |
| Clawbacks | $2.50 | FR-016 |
| Earned commission | $247.51 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $247.51 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $11,752.49 | FR-015 |

**Yael**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| X-1 booked 2026-01-07 (closed 2026-01-06): 50% share of $10,000.20 | $5,000.10 | FR-012 |
| Credited bookings | $5,000.10 | FR-008 |
| 5% of $5,000.10 | $250.01 | FR-006 |
| Commission before refunds | $250.01 | FR-006 |
| X-1 re-split after refund on 2026-02-10: share of $9,900.19 | $4,950.09 | FR-017 |
| Clawback: X-1 refund $100.01 on 2026-02-10 | $2.51 | FR-016 |
| Clawbacks | $2.51 | FR-016 |
| Earned commission | $247.50 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $247.50 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $11,752.50 | FR-015 |

**Ari**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| X-3 booked 2026-01-09 (closed 2026-01-08): 45% share of $0.06 | $0.03 | FR-012 |
| Credited bookings | $0.03 | FR-008 |
| 5% of $0.03 | $0.00 | FR-006 |
| Commission before refunds | $0.00 | FR-006 |
| X-3 re-split after refund on 2026-02-11: share of $0.05 | $0.02 | FR-017 |
| Clawback: X-3 refund $0.01 on 2026-02-11 | $0.00 | FR-016 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $0.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $0.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $12,000.00 | FR-015 |

**Bo**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| X-3 booked 2026-01-09 (closed 2026-01-08): 45% share of $0.06 | $0.03 | FR-012 |
| Credited bookings | $0.03 | FR-008 |
| 5% of $0.03 | $0.00 | FR-006 |
| Commission before refunds | $0.00 | FR-006 |
| X-3 re-split after refund on 2026-02-11: share of $0.05 | $0.02 | FR-017 |
| Clawback: X-3 refund $0.01 on 2026-02-11 | $0.00 | FR-016 |
| Clawbacks | $0.00 | FR-016 |
| Earned commission | $0.00 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $0.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $12,000.00 | FR-015 |

**Cy**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| X-2 booked 2026-01-07 (closed 2026-01-06) | $10.09 | FR-008 |
| X-3 booked 2026-01-09 (closed 2026-01-08): 10% share of $0.06 | $0.00 | FR-012 |
| Credited bookings | $10.09 | FR-008 |
| 5% of $10.09 | $0.50 | FR-006 |
| Commission before refunds | $0.50 | FR-006 |
| X-3 re-split after refund on 2026-02-11: share of $0.05 | $0.01 | FR-017 |
| Clawback: X-3 refund $0.01 on 2026-02-11 | −$0.01 | FR-016 |
| Clawbacks | −$0.01 | FR-016 |
| Earned commission | $0.51 | FR-015 |
| Draw, January 2026 | $4,000.00 | FR-014 |
| Draw, February 2026 | $4,000.00 | FR-014 |
| Draw, March 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $0.51 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $11,999.49 | FR-015 |

### A.10 `refunds-q2`: Refunds of earlier-quarter deals

Quarter 2026-04-01 to 2026-06-30 (91 days). Carries US6 AS2, AS5 (Q2), AS8.

| Rep | Quota | Start date | Opening balance |
|---|---|---|---|
| Sage (`sage`) | $100,000.00 | 2025-06-01 | $0.00 |
| Val (`val`) | $100,000.00 | 2025-06-01 | $0.00 |
| Zion (`zion`) | $100,000.00 | 2025-06-01 | $0.00 |

Deals:

| Deal | Amount | Close date | Booking date | Credit | Refunds |
|---|---|---|---|---|---|
| Q2-1 | $40,000.00 | 2026-05-01 | 2026-05-04 | sage 100% | — |

Booking-quarter data, 2026-01-01 to 2026-03-31: reps `sage` (quota $100,000.00, start 2025-06-01), `val` (quota $100,000.00, start 2025-06-01), `zion` (quota $100,000.00, start 2025-06-01); partners not on the roster: `yves` (start 2025-06-01).

| Deal | Amount | Close date | Booking date | Credit | Refunds |
|---|---|---|---|---|---|
| R-11 | $60,000.00 | 2026-01-09 | 2026-01-10 | sage 100% | $60,000.00 on 2026-04-15 |
| R-12 | $60,000.00 | 2026-02-09 | 2026-02-10 | sage 100% | — |
| R-7 | $60,000.00 | 2026-01-09 | 2026-01-10 | val 100% | $20,000.00 on 2026-02-20, $20,000.00 on 2026-05-05 |
| R-8 | $60,000.00 | 2026-02-09 | 2026-02-10 | val 100% | — |
| D1 | $60,000.00 | 2026-01-14 | 2026-01-15 | zion 100% | $60,000.00 on 2026-04-20 |
| D2 | $40,000.00 | 2026-02-24 | 2026-02-25 | zion 50%, yves 50% | — |

**Sage**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| Q2-1 booked 2026-05-04 (closed 2026-05-01) | $40,000.00 | FR-008 |
| Credited bookings | $40,000.00 | FR-008 |
| 5% of $40,000.00 | $2,000.00 | FR-006 |
| Commission before refunds | $2,000.00 | FR-006 |
| Clawback: R-11 refund $60,000.00 on 2026-04-15 | $3,600.00 | FR-016 |
| Clawbacks | $3,600.00 | FR-016 |
| Earned commission | −$1,600.00 | FR-015 |
| Draw, April 2026 | $4,000.00 | FR-014 |
| Draw, May 2026 | $4,000.00 | FR-014 |
| Draw, June 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $0.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $13,600.00 | FR-015 |

**Val**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| Credited bookings | $0.00 | FR-008 |
| Commission before refunds | $0.00 | FR-006 |
| Clawback: R-7 refund $20,000.00 on 2026-05-05 | $1,000.00 | FR-016 |
| Clawbacks | $1,000.00 | FR-016 |
| Earned commission | −$1,000.00 | FR-015 |
| Draw, April 2026 | $4,000.00 | FR-014 |
| Draw, May 2026 | $4,000.00 | FR-014 |
| Draw, June 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $0.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $13,000.00 | FR-015 |

**Zion**

| Item | Amount | Rule |
|---|---|---|
| Quarterly quota | $100,000.00 | FR-005 |
| Credited bookings | $0.00 | FR-008 |
| Commission before refunds | $0.00 | FR-006 |
| Clawback: D1 refund $60,000.00 on 2026-04-20 | $3,000.00 | FR-016 |
| Clawbacks | $3,000.00 | FR-016 |
| Earned commission | −$3,000.00 | FR-015 |
| Draw, April 2026 | $4,000.00 | FR-014 |
| Draw, May 2026 | $4,000.00 | FR-014 |
| Draw, June 2026 | $4,000.00 | FR-014 |
| Draws paid | $12,000.00 | FR-014 |
| Opening recoverable balance | $0.00 | FR-015 |
| Draw recovered | $0.00 | FR-015 |
| Commission payable | $0.00 | FR-015 |
| Closing recoverable balance | $15,000.00 | FR-015 |

### A.11 `invalid`: Invalid data (rejected)

Quarter 2026-01-01 to 2026-03-31. Carries US4 AS5 and the FR-011 edge cases. The scenario MUST be
rejected with all four errors listed and no statements (FR-004):

| Rep | Quota | Start date |
|---|---|---|
| Mara (`mara`) | $100,000.00 | 2025-06-01 |
| Nico (`nico`) | $100,000.00 | 2026-02-01 |
| Ollie (`ollie`) | $100,000.00 | 2026-04-10 |

| Deal | Amount | Close date | Booking date | Credit | Refunds |
|---|---|---|---|---|---|
| V-1 | $50,000.00 | 2026-02-01 | 2026-02-02 | mara 60%, nico 30% | — |
| V-2 | $10,000.00 | 2026-01-14 | 2026-01-15 | nico 100% | — |
| V-3 | $10.005 | 2026-03-01 | 2026-03-02 | mara 100% | — |

Expected errors: V-1's split percentages sum to 90% (FR-013); V-2 is booked 2026-01-15, before
Nico's start date of 2026-02-01 (FR-011); Ollie starts 2026-04-10, after the quarter ends
(FR-011); V-3's amount $10.005 is not a whole number of cents (FR-004).
