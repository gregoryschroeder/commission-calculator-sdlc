@FR-006 @FR-007 @FR-009 @FR-018
Feature: US1 - Tiered commission on quota attainment
  Commission is 5% up to 100% of quota, 8% from 100% to 150%, and 12% above 150%, applied
  marginally. Line descriptions are Appendix A.1's, exactly.

  Background:
    Given the quarter 2026-01-01 to 2026-03-31

  Scenario: US1 AS1 - 80% attainment earns 5% on every dollar
    Given the roster:
      | rep   | name  | quota      | start      |
      | avery | Avery | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit     |
      | T-1  | 80,000.00 | 2026-02-10 | 2026-02-12 | avery 100% |
    When the scenario is calculated
    Then the statement for "avery" includes, in order:
      | item                                      | amount    | rule   |
      | T-1 booked 2026-02-12 (closed 2026-02-10) | 80,000.00 | FR-008 |
      | Credited bookings                         | 80,000.00 | FR-008 |
      | 5% of $80,000.00                          | 4,000.00  | FR-006 |
      | Commission before refunds                 | 4,000.00  | FR-006 |
    And the statement for "avery" has:
      | field             | value    |
      | Attainment        | 0.80     |
      | CreditedBookings  | 80,000.00 |
      | EarnedCommission  | 4,000.00 |

  Scenario: US1 AS2 - 160% attainment earns each band's rate on its own slice
    Given the roster:
      | rep   | name  | quota      | start      |
      | blake | Blake | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount     | close      | booking    | credit     |
      | T-2  | 100,000.00 | 2026-01-20 | 2026-01-22 | blake 100% |
      | T-3  | 60,000.00  | 2026-03-02 | 2026-03-04 | blake 100% |
    When the scenario is calculated
    Then the statement for "blake" includes, in order:
      | item                      | amount     | rule   |
      | Credited bookings         | 160,000.00 | FR-008 |
      | 5% of $100,000.00         | 5,000.00   | FR-006 |
      | 8% of $50,000.00          | 4,000.00   | FR-006 |
      | 12% of $10,000.00         | 1,200.00   | FR-006 |
      | Commission before refunds | 10,200.00  | FR-006 |
    And the statement for "blake" has:
      | field            | value     |
      | EarnedCommission | 10,200.00 |

  Scenario: US1 AS3 - exactly 150% attainment has no 12% line
    Given the roster:
      | rep   | name  | quota      | start      |
      | casey | Casey | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount     | close      | booking    | credit     |
      | T-4  | 150,000.00 | 2026-02-26 | 2026-03-02 | casey 100% |
    When the scenario is calculated
    Then the statement for "casey" includes, in order:
      | item                      | amount   | rule   |
      | 5% of $100,000.00         | 5,000.00 | FR-006 |
      | 8% of $50,000.00          | 4,000.00 | FR-006 |
      | Commission before refunds | 9,000.00 | FR-006 |
    And the statement for "casey" has no line starting "12% of"
    And the statement for "casey" has:
      | field            | value    |
      | EarnedCommission | 9,000.00 |

  Scenario: US1 AS4 - half a cent rounds away from zero
    Given the roster:
      | rep   | name  | quota      | start      |
      | devon | Devon | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount | close      | booking    | credit     |
      | T-5  | 10.10  | 2026-01-15 | 2026-01-16 | devon 100% |
    When the scenario is calculated
    Then the statement for "devon" includes, in order:
      | item                      | amount | rule   |
      | 5% of $10.10              | 0.51   | FR-006 |
      | Commission before refunds | 0.51   | FR-006 |
    And the statement for "devon" has:
      | field            | value |
      | EarnedCommission | 0.51  |
