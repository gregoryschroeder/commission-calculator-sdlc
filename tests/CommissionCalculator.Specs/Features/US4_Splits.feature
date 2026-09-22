@FR-012 @FR-013 @FR-009
Feature: US4 - A deal may be split between reps
  Each rep's split share counts toward their own attainment and commission, and only the share
  (clarification 2026-09-21). Shares are allocated to the cent by largest remainder. Line
  descriptions are Appendix A.5's and A.6's, exactly.

  Background:
    Given the quarter 2026-01-01 to 2026-03-31

  Scenario: US4 AS1 - each rep is credited with their own share
    Given the roster:
      | rep    | name   | quota      | start      |
      | indigo | Indigo | 100,000.00 | 2025-06-01 |
      | jules  | Jules  | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit                 |
      | S-2  | 50,000.00 | 2026-02-17 | 2026-02-18 | indigo 60%, jules 40%  |
    When the scenario is calculated
    Then the statement for "indigo" includes, in order:
      | item                                                              | amount    | rule   |
      | S-2 booked 2026-02-18 (closed 2026-02-17): 60% share of $50,000.00 | 30,000.00 | FR-012 |
    And the statement for "jules" includes, in order:
      | item                                                              | amount    | rule   |
      | S-2 booked 2026-02-18 (closed 2026-02-17): 40% share of $50,000.00 | 20,000.00 | FR-012 |
    And the statement for "jules" has:
      | field            | value     |
      | CreditedBookings | 20,000.00 |

  Scenario: US4 AS2 - a share counts toward attainment at the share amount only
    Given the roster:
      | rep    | name   | quota      | start      |
      | indigo | Indigo | 100,000.00 | 2025-06-01 |
      | jules  | Jules  | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit                |
      | S-1  | 80,000.00 | 2026-01-12 | 2026-01-13 | indigo 100%           |
      | S-2  | 50,000.00 | 2026-02-17 | 2026-02-18 | indigo 60%, jules 40% |
    When the scenario is calculated
    Then the statement for "indigo" includes, in order:
      | item                      | amount     | rule   |
      | Credited bookings         | 110,000.00 | FR-008 |
      | 5% of $100,000.00         | 5,000.00   | FR-006 |
      | 8% of $10,000.00          | 800.00     | FR-006 |
      | Commission before refunds | 5,800.00   | FR-006 |
    And the statement for "indigo" has:
      | field            | value      |
      | CreditedBookings | 110,000.00 |
      | Attainment       | 1.10       |
      | EarnedCommission | 5,800.00   |

  Scenario: US4 AS3 - the leftover cent goes to the rep listed first
    Given the roster:
      | rep | name | quota      | start      |
      | kai | Kai  | 100,000.00 | 2025-06-01 |
      | lee | Lee  | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount | close      | booking    | credit            |
      | S-3  | 10.01  | 2026-02-02 | 2026-02-03 | kai 50%, lee 50%  |
    When the scenario is calculated
    Then the statement for "kai" includes, in order:
      | item                                                        | amount | rule   |
      | S-3 booked 2026-02-03 (closed 2026-02-02): 50% share of $10.01 | 5.01   | FR-012 |
    And the statement for "lee" includes, in order:
      | item                                                        | amount | rule   |
      | S-3 booked 2026-02-03 (closed 2026-02-02): 50% share of $10.01 | 5.00   | FR-012 |

  Scenario: US4 AS4 - three shares still sum to the deal
    Given the roster:
      | rep | name | quota      | start      |
      | mo  | Mo   | 100,000.00 | 2025-06-01 |
      | nat | Nat  | 100,000.00 | 2025-06-01 |
      | oli | Oli  | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount | close      | booking    | credit                              |
      | S-4  | 100.00 | 2026-03-05 | 2026-03-06 | mo 33.335%, nat 33.335%, oli 33.33% |
    When the scenario is calculated
    Then the statement for "mo" has:
      | field            | value |
      | CreditedBookings | 33.34 |
    And the statement for "nat" has:
      | field            | value |
      | CreditedBookings | 33.33 |
    And the statement for "oli" has:
      | field            | value |
      | CreditedBookings | 33.33 |

  Scenario: US4 AS5 - split percentages that do not sum to 100% reject the scenario
    Given the roster:
      | rep  | name | quota      | start      |
      | mara | Mara | 100,000.00 | 2025-06-01 |
      | nico | Nico | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit              |
      | V-1  | 50,000.00 | 2026-02-01 | 2026-02-02 | mara 60%, nico 30%  |
    When the scenario is calculated it is rejected
    Then the rejection names deal "V-1" and cites "FR-013"
    And the rejection says "split percentages sum to 90%"
