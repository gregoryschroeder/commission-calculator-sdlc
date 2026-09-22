@FR-010 @FR-009 @FR-018
Feature: US3 - A mid-quarter starter is measured against a prorated quota
  The quota is prorated by calendar days from the start date to the quarter's last day, both
  inclusive (clarification 2026-09-21). Line descriptions are Appendix A.3's and A.4's, exactly.

  Scenario: US3 AS1 - 45 of 90 days halves the quota
    Given the quarter 2026-01-01 to 2026-03-31
    And the roster:
      | rep    | name   | quota     | start      |
      | finley | Finley | 90,000.00 | 2026-02-15 |
    And the deals:
      | deal | amount    | close      | booking    | credit      |
      | P-1  | 50,000.00 | 2026-03-09 | 2026-03-10 | finley 100% |
    When the scenario is calculated
    Then the statement for "finley" includes, in order:
      | item                           | amount    | rule   |
      | Quarterly quota                | 90,000.00 | FR-005 |
      | Prorated quota (45 of 90 days) | 45,000.00 | FR-010 |
    And the statement for "finley" has:
      | field         | value     |
      | ProratedQuota | 45,000.00 |

  Scenario: US3 AS2 - attainment and commission use the prorated quota
    Given the quarter 2026-01-01 to 2026-03-31
    And the roster:
      | rep    | name   | quota     | start      |
      | finley | Finley | 90,000.00 | 2026-02-15 |
    And the deals:
      | deal | amount    | close      | booking    | credit      |
      | P-1  | 50,000.00 | 2026-03-09 | 2026-03-10 | finley 100% |
    When the scenario is calculated
    Then the statement for "finley" includes, in order:
      | item                      | amount    | rule   |
      | 5% of $45,000.00          | 2,250.00  | FR-006 |
      | 8% of $5,000.00           | 400.00    | FR-006 |
      | Commission before refunds | 2,650.00  | FR-006 |

  Scenario: US3 AS3 - a prorated quota that does not divide evenly is rounded to the cent
    Given the quarter 2026-04-01 to 2026-06-30
    And the roster:
      | rep    | name   | quota      | start      |
      | harper | Harper | 100,000.00 | 2026-05-16 |
    And the deals:
      | deal | amount    | close      | booking    | credit      |
      | P-3  | 60,000.00 | 2026-06-01 | 2026-06-03 | harper 100% |
    When the scenario is calculated
    Then the statement for "harper" includes, in order:
      | item                           | amount    | rule   |
      | Prorated quota (46 of 91 days) | 50,549.45 | FR-010 |
      | 5% of $50,549.45               | 2,527.47  | FR-006 |
      | 8% of $9,450.55                | 756.04    | FR-006 |
      | Commission before refunds      | 3,283.51  | FR-006 |

  Scenario: US3 AS4 - starting on or before the first day is not prorated
    Given the quarter 2026-01-01 to 2026-03-31
    And the roster:
      | rep   | name  | quota      | start      |
      | avery | Avery | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit     |
      | T-1  | 80,000.00 | 2026-02-10 | 2026-02-12 | avery 100% |
    When the scenario is calculated
    Then the statement for "avery" has no line starting "Prorated quota"
    And the statement for "avery" has:
      | field         | value      |
      | ProratedQuota | 100,000.00 |
