# Data Model: Quarterly Sales Commission Calculator

All money is an exact decimal in USD with at most two decimal places on input (FR-004). Dates are
calendar dates with no time or time zone. Percentages are exact decimals (e.g. `33.335`).

## Input entities (engine inputs, read from a scenario file)

### Scenario
| Field | Type | Rules |
|---|---|---|
| `id` | string | Unique across seeded scenarios, else a load error naming it (FR-004); used in the page URL. |
| `name`, `description` | string | Shown in the picker and page heading. |
| `quarter` | Quarter | Must be three whole calendar months starting on the 1st (Edge Cases). |
| `roster` | Rep[] | Reps who receive a statement. At least one; unique `repId` (FR-004). |
| `deals` | Deal[] | The scenario's own deal list, in listed order (FR-008 lists excluded ones). |
| `bookingQuarters` | BookingQuarter[] | Earlier quarters needed to size clawbacks (FR-016). May be empty. |

### Quarter
| Field | Type | Rules |
|---|---|---|
| `start`, `end` | date | `start` is the 1st of a month; `end` is the last day of the third month. |

Derived: `Days = end − start + 1`; `Months` = the three calendar months.

### Rep
| Field | Type | Rules |
|---|---|---|
| `repId`, `name` | string | |
| `quota` | money | > 0, whole cents (FR-004, FR-005). |
| `startDate` | date | On or before `quarter.end` (FR-011). |
| `openingRecoverableBalance` | money | ≥ 0, whole cents, default 0.00 (FR-004, FR-015). |

### Deal
| Field | Type | Rules |
|---|---|---|
| `dealId` | string | Unique within its quarter's deal list (FR-004). |
| `amount` | money | > 0, whole cents. |
| `closeDate` | date | Informational only (FR-008). |
| `bookingDate` | date | Decides the quarter (FR-008). |
| `splits` | SplitCredit[] | ≥ 1; percentages sum to exactly 100 (FR-013); each rep at most once (FR-004); every rep's start date ≤ `bookingDate` (FR-011). |
| `refunds` | Refund[] | In listed order; total ≤ `amount` (FR-017). |

### SplitCredit
| Field | Type | Rules |
|---|---|---|
| `repId` | string | For the scenario's own deals: on the roster (FR-004). For booking-quarter deals: on the roster or in that quarter's `partners`. |
| `percent` | decimal | > 0 and ≤ 100 (FR-004). |

### Refund
| Field | Type | Rules |
|---|---|---|
| `amount` | money | > 0, whole cents. |
| `date` | date | ≥ the deal's `bookingDate`. |

### BookingQuarter
| Field | Type | Rules |
|---|---|---|
| `quarter` | Quarter | Three whole calendar months; must not overlap the scenario quarter (FR-004). |
| `reps` | {repId, quota, startDate}[] | For roster reps credited on its deals. `startDate` must equal the roster's (FR-004). |
| `partners` | {repId, startDate}[] | Split partners not on the roster; they get no statement (FR-016). |
| `deals` | Deal[] | All deals booked in it for those reps (each `bookingDate` inside it), with every refund dated before the refund being sized (FR-004). |

Completeness is checked, as far as the engine can detect it, for every deal that has a refund dated
in the scenario quarter and a booking date before it: its booking quarter must be present, each
credited roster rep must have a `reps` entry and each non-roster partner on any deal in that booking quarter a `partners` entry (FR-011 checks their
start dates),
otherwise the scenario is rejected (FR-004). That the booking quarter lists *all* of those reps'
deals and every earlier refund is a precondition on the data, not a check (FR-004, clarification
2026-09-22). Such a deal is sized from the booking quarter's copy;
if the scenario's own deal list also lists it (as an excluded deal), the two entries must be
identical, otherwise the data is inconsistent and the scenario is rejected (FR-004).

## Output entities (engine results)

### ScenarioResult
Either **Rejected** — a list of `ValidationError {message, requirementId}` (FR-004), no statements
— or **Calculated** — one `RepStatement` per roster rep, in roster order.

### RepStatement
| Field | Meaning | FR |
|---|---|---|
| `quota`, `proratedQuota` | Full and prorated quota | FR-005, FR-010 |
| `creditedBookings` | Sum of counted credit lines (before refunds) | FR-008, FR-012 |
| `attainment` | `creditedBookings ÷ proratedQuota`, display only (never used for an amount) | FR-009 |
| `commissionBeforeRefunds` | Sum of tier lines | FR-006 |
| `clawbacks` | Sum of clawback lines (may be negative) | FR-016, FR-017 |
| `earnedCommission` | `commissionBeforeRefunds − clawbacks` (may be negative) | FR-015 |
| `drawPaid` | Sum of monthly draw lines | FR-014 |
| `recovered`, `payable`, `closingRecoverableBalance` | Per FR-015 | FR-015 |
| `lines` | Ordered `BreakdownLine`s | FR-003 |

### BreakdownLine
`{ section, description, amount, requirementId }`. Every amount shown for a rep is a line; each
subtotal equals the sum of the lines above it that it names (FR-018). `requirementId` is always
an FR in spec.md (SC-002).

## Calculation (per roster rep)

Definitions used below: `R(x) = round half away from zero to the cent` (FR-018).

1. **Credit in a quarter Q under an applied-refund set A** — for each deal booked in Q whose
   splits include the rep: `reduced = amount − Σ(refunds of that deal in A)`; allocate `reduced`
   across the deal's splits by largest remainder (FR-012); the rep's share is their credit from
   that deal. Credit = Σ shares.
2. **Prorated quota** (FR-010) — if `startDate > Q.start`:
   `R(quota × (Q.end − startDate + 1) ÷ Q.Days)`, else `quota`. Must not be 0.00 (FR-004).
3. **Commission(Q, A)** (FR-006, FR-007) — `e100 = proratedQuota`, `e150 = R(1.5 × proratedQuota)`;
   lines `R(5% × min(credit, e100))`, `R(8% × clamp(credit − e100, 0, e150 − e100))`,
   `R(12% × max(credit − e150, 0))`, omitting zero slices; commission = Σ lines.
4. **Commission before refunds** = Commission(scenario quarter, ∅). (A refund cannot predate its
   deal's booking date, so no refund applies to a scenario-quarter deal before the quarter.)
5. **Clawbacks** (FR-016, FR-017) — for each booking quarter BQ (the scenario quarter itself, or
   an earlier `bookingQuarter`), order every refund on BQ's deals credited to the rep by
   (date, deal position, refund position). For each refund r dated inside the scenario quarter:
   `clawback(r) = Commission(BQ, earlier(r)) − Commission(BQ, earlier(r) ∪ {r})`, where
   `earlier(r)` is every refund before r in that order. Refunds dated after the scenario quarter
   are ignored for amounts.
6. **Draw** (FR-014) — for each month of the quarter: 0 before the start month; in the start
   month (if the start date is after the 1st) `R(4000 × daysEmployedInMonth ÷ daysInMonth)`;
   otherwise 4,000.00.
7. **Recovery** (FR-015) — `recoverableTotal = opening + drawPaid`; if `earned ≥ 0`:
   `recovered = min(earned, recoverableTotal)`, `payable = earned − recovered`,
   `closing = recoverableTotal − recovered`; else `recovered = payable = 0`,
   `closing = recoverableTotal − earned`.

## Validation (scenario is rejected, with every error listed, if any fails)

FR-004 (whole cents, positives, non-negative opening balance, split percent range, rep once per
deal, prorated quota ≠ 0, roster membership, booking-quarter completeness and consistency,
quarter shape, refund date ≥ booking date, refunds ≤ amount), FR-011 (start dates), FR-013
(split sum). Validation runs before any calculation; a rejected scenario has no statements.
