@FR-008
Feature: US2 - The booking date decides the quarter
  A deal counts toward the quarter that contains its booking date; its close date is shown for
  information only. Line descriptions are Appendix A.2's, exactly.

  Background:
    Given the quarter 2026-01-01 to 2026-03-31
    And the roster:
      | rep   | name  | quota      | start      |
      | emery | Emery | 100,000.00 | 2025-01-01 |

  Scenario: US2 AS1 - booked after the quarter, so excluded even though it closed inside it
    Given the deals:
      | deal | amount    | close      | booking    | credit     |
      | B-1  | 20,000.00 | 2026-03-30 | 2026-04-02 | emery 100% |
    When the scenario is calculated
    Then the statement for "emery" includes, in order:
      | item                                                                            | amount | rule   |
      | B-1 booked 2026-04-02 (closed 2026-03-30): excluded, booked outside the quarter | 0.00   | FR-008 |
      | Credited bookings                                                               | 0.00   | FR-008 |
    And the statement for "emery" has:
      | field            | value |
      | CreditedBookings | 0.00  |

  Scenario: US2 AS2 - closed before the quarter but booked inside it, so counted
    Given the deals:
      | deal | amount    | close      | booking    | credit     |
      | B-2  | 10,000.00 | 2025-12-29 | 2026-01-05 | emery 100% |
    When the scenario is calculated
    Then the statement for "emery" includes, in order:
      | item                                      | amount    | rule   |
      | B-2 booked 2026-01-05 (closed 2025-12-29) | 10,000.00 | FR-008 |
      | Credited bookings                         | 10,000.00 | FR-008 |

  Scenario: US2 AS3 - both dates inside the quarter, so counted
    Given the deals:
      | deal | amount    | close      | booking    | credit     |
      | B-3  | 30,000.00 | 2026-02-03 | 2026-02-04 | emery 100% |
    When the scenario is calculated
    Then the statement for "emery" includes, in order:
      | item                                      | amount    | rule   |
      | B-3 booked 2026-02-04 (closed 2026-02-03) | 30,000.00 | FR-008 |
      | Credited bookings                         | 30,000.00 | FR-008 |

  Scenario: US2 AS4 - both dates outside the quarter, so excluded with the reason
    Given the deals:
      | deal | amount    | close      | booking    | credit     |
      | B-4  | 15,000.00 | 2025-12-10 | 2025-12-15 | emery 100% |
    When the scenario is calculated
    Then the statement for "emery" includes, in order:
      | item                                                                            | amount | rule   |
      | B-4 booked 2025-12-15 (closed 2025-12-10): excluded, booked outside the quarter | 0.00   | FR-008 |
      | Credited bookings                                                               | 0.00   | FR-008 |
