# Feature Specification: Quarterly Sales Commission Calculator

**Feature Branch**: `001-commission-calculator`

**Created**: 2026-09-21

**Status**: Draft

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

## User Scenarios & Testing *(mandatory)*

The user is anyone checking a quarterly commission statement: a sales-operations analyst, a
manager, or a rep. They pick one of the seeded scenarios and read, for every rep in it, what the
rep is paid and exactly which rule produced every amount.

Every dollar amount in an acceptance scenario below is a requirement (Principle II: each is
tested with its exact inputs, to the cent). All payout-affecting ambiguities were resolved by
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
- A scenario quarter that is not exactly three whole calendar months starting on the first day of
  a month: rejected (FR-014 counts draws per calendar month).
- A refund dated before the deal's booking date: rejected.
- A rep whose start date is after the quarter's last day: the scenario is rejected, naming the rep
  (FR-011).
- A deal booked before the start date of a rep credited on it, whether or not the deal counts this
  quarter: the scenario is rejected, naming the deal and the rep (FR-011).
- Amounts that do not divide to whole cents (e.g. 5% of $10.10 is $0.505): rounded per FR-018.
- A partial refund: clawed back per FR-017. A refund of zero or less, or refunds on one deal
  totalling more than the deal amount, are rejected.
- A clawback larger than the commission earned in the quarter: earned commission goes negative
  and the magnitude is added to the closing recoverable balance (FR-015, FR-016).

## Requirements *(mandatory)*

### Functional Requirements

**Scenarios and display**

- **FR-001**: The system MUST offer a list of seeded scenarios and let the user select one.
- **FR-002**: For the selected scenario, the system MUST show every rep in it with their quota,
  credited bookings, attainment, earned commission, draw, recovery, clawbacks, commission payable
  and closing recoverable balance.
- **FR-003**: Each rep's result MUST include a line-by-line breakdown in which every amount is on
  its own line and each line cites the FR ID of the rule that produced it.
- **FR-004**: A scenario that violates a validation rule (FR-011, FR-013, Edge Cases) MUST be shown
  as rejected with the reasons, and MUST NOT show any payout. Every monetary input MUST be a whole
  number of cents; deal amounts, quotas and refunds MUST be greater than zero; an opening
  recoverable balance MUST be zero or more; every split percentage MUST be greater than 0% and at
  most 100%. A scenario MUST also be rejected, naming the rep, deal or quarter involved, when: a
  rep appears more than once on one deal; a prorated quota rounds to $0.00; any deal in the
  scenario's own deal list (counted or not) is credited to a rep not on the roster; a deal
  refunded in the quarter was booked in an earlier quarter whose data is missing or incomplete
  (complete means: for each roster rep credited on the deal, that quarter's dates, their quota,
  start date, all their deals booked in it, and every refund on those deals dated before the
  refund being sized, whatever quarter that earlier refund falls in; for a split partner not on the
  roster, their start date), is not three whole calendar months, overlaps the scenario's quarter, or
  contains a deal booked outside its own dates; or a rep's start date in booking-quarter data
  differs from the roster's.

**Quota and rates**

- **FR-005**: Each rep MUST have a quarterly quota in USD, greater than zero.
- **FR-006**: Commission MUST be tiered on quota attainment at 5% (requirement) up to 100%
  (requirement) of quota, 8% (requirement) from 100% to 150% (requirement), and 12%
  (requirement) above 150%, applied marginally: each slice of credited bookings earns its own
  band's rate, and each band with a non-zero slice is its own breakdown line.
- **FR-007**: Band edges are at exactly 100% and 150% of quota: the 5% band covers credited
  bookings up to and including 100% of quota, the 8% band the part above 100% up to and including
  150%, and the 12% band the part above 150%. (Under marginal tiers the edge assignment cannot
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
  draw is $4,000.00 × (days from the start date to that month's last day, both inclusive) ÷ (days
  in that month); months before the start month carry no draw. Each month's draw is its own
  breakdown line.
- **FR-015**: Each rep has an opening recoverable balance (default $0.00) of draw not yet
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

- **FR-018**: Every computed monetary amount — prorated quota, the 150% band edge,
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

- **SC-001**: For 100% (requirement) of the seeded scenarios, every rep's final amount and every
  breakdown line matches the expected value stated in this spec, to the cent.
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
