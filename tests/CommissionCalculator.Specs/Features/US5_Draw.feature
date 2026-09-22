@FR-014 @FR-015
Feature: US5 - A monthly draw, recovered against commission
  Every rep draws $4,000.00 a month, prorated in the month they start. The draw is recovered
  against earned commission; what is left over is carried forward and what is over the draw is
  payable (clarification 2026-09-21). Line descriptions are Appendix A.7's and A.3's, exactly.

  Background:
    Given the quarter 2026-01-01 to 2026-03-31

  Scenario: US5 AS1 - commission above the quarter's draw is payable
    Given the roster:
      | rep    | name   | quota      | start      |
      | parker | Parker | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount     | close      | booking    | credit      |
      | W-1  | 200,000.00 | 2026-02-20 | 2026-02-23 | parker 100% |
    When the scenario is calculated
    Then the statement for "parker" includes, in order:
      | item                        | amount    | rule   |
      | Draw, January 2026          | 4,000.00  | FR-014 |
      | Draw, February 2026         | 4,000.00  | FR-014 |
      | Draw, March 2026            | 4,000.00  | FR-014 |
      | Draws paid                  | 12,000.00 | FR-014 |
      | Opening recoverable balance | 0.00      | FR-015 |
      | Draw recovered              | 12,000.00 | FR-015 |
      | Commission payable          | 3,000.00  | FR-015 |
      | Closing recoverable balance | 0.00      | FR-015 |
    And the statement for "parker" has:
      | field   | value    |
      | Payable | 3,000.00 |

  Scenario: US5 AS2 - commission below the draw carries the shortfall forward
    Given the roster:
      | rep   | name  | quota      | start      |
      | quinn | Quinn | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit     |
      | W-2  | 80,000.00 | 2026-03-11 | 2026-03-12 | quinn 100% |
    When the scenario is calculated
    Then the statement for "quinn" includes, in order:
      | item                        | amount    | rule   |
      | Draw recovered              | 4,000.00  | FR-015 |
      | Commission payable          | 0.00      | FR-015 |
      | Closing recoverable balance | 8,000.00  | FR-015 |

  Scenario: US5 AS3 - an opening balance is recovered before anything is payable
    Given the roster:
      | rep   | name  | quota      | start      | opening  |
      | reese | Reese | 100,000.00 | 2025-06-01 | 8,000.00 |
    And the deals:
      | deal | amount     | close      | booking    | credit     |
      | W-3  | 200,000.00 | 2026-01-26 | 2026-01-27 | reese 100% |
    When the scenario is calculated
    Then the statement for "reese" includes, in order:
      | item                        | amount    | rule   |
      | Opening recoverable balance | 8,000.00  | FR-015 |
      | Draw recovered              | 15,000.00 | FR-015 |
      | Commission payable          | 0.00      | FR-015 |
      | Closing recoverable balance | 5,000.00  | FR-015 |

  Scenario: US5 AS4 and AS5 - a mid-quarter starter draws a prorated start month
    Given the roster:
      | rep    | name   | quota     | start      |
      | finley | Finley | 90,000.00 | 2026-02-15 |
    And the deals:
      | deal | amount    | close      | booking    | credit      |
      | P-1  | 50,000.00 | 2026-03-09 | 2026-03-10 | finley 100% |
    When the scenario is calculated
    Then the statement for "finley" includes, in order:
      | item                        | amount   | rule   |
      | Draw, January 2026          | 0.00     | FR-014 |
      | Draw, February 2026         | 2,000.00 | FR-014 |
      | Draw, March 2026            | 4,000.00 | FR-014 |
      | Draws paid                  | 6,000.00 | FR-014 |
      | Draw recovered              | 2,650.00 | FR-015 |
      | Commission payable          | 0.00     | FR-015 |
      | Closing recoverable balance | 3,350.00 | FR-015 |

  Scenario: US5 AS6 - the start month's draw is prorated by its own days
    Given the roster:
      | rep  | name | quota     | start      |
      | gray | Gray | 90,000.00 | 2026-01-20 |
    And the deals:
      | deal | amount    | close      | booking    | credit    |
      | P-2  | 30,000.00 | 2026-01-30 | 2026-02-02 | gray 100% |
    When the scenario is calculated
    Then the statement for "gray" includes, in order:
      | item               | amount   | rule   |
      | Draw, January 2026 | 1,548.39 | FR-014 |
      | Draws paid         | 9,548.39 | FR-014 |
    And the statement for "gray" has:
      | field    | value    |
      | DrawPaid | 9,548.39 |
