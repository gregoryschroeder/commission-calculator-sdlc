@FR-016 @FR-017
Feature: US6 - A refunded deal is clawed back
  A clawback is the booking quarter recomputed with and without the refunded revenue, shown in the
  statement of the quarter the refund falls in (clarifications 2026-09-21). Line descriptions are
  Appendix A.8's, A.9's and A.10's, exactly.

  Scenario: US6 AS1 - a full refund inside the quarter
    Given the quarter 2026-01-01 to 2026-03-31
    And the roster:
      | rep  | name | quota      | start      |
      | sage | Sage | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit    | refunds                    |
      | R-1  | 60,000.00 | 2026-01-09 | 2026-01-10 | sage 100% | 60,000.00 on 2026-02-15    |
      | R-2  | 60,000.00 | 2026-02-09 | 2026-02-10 | sage 100% |                            |
    When the scenario is calculated
    Then the statement for "sage" includes, in order:
      | item                                          | amount     | rule   |
      | Commission before refunds                     | 6,600.00   | FR-006 |
      | Clawback: R-1 refund $60,000.00 on 2026-02-15 | 3,600.00   | FR-016 |
      | Clawbacks                                     | 3,600.00   | FR-016 |
      | Earned commission                             | 3,000.00   | FR-015 |

  Scenario: US6 AS2 - a refund of an earlier quarter's deal lands in this quarter
    Given the quarter 2026-04-01 to 2026-06-30
    And the roster:
      | rep  | name | quota      | start      |
      | sage | Sage | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit    | refunds |
      | Q2-1 | 40,000.00 | 2026-05-01 | 2026-05-04 | sage 100% |         |
    And the booking quarter 2026-01-01 to 2026-03-31 with reps:
      | rep  | quota      | start      |
      | sage | 100,000.00 | 2025-06-01 |
    And its deals:
      | deal | amount    | close      | booking    | credit    | refunds                 |
      | R-11 | 60,000.00 | 2026-01-09 | 2026-01-10 | sage 100% | 60,000.00 on 2026-04-15 |
      | R-12 | 60,000.00 | 2026-02-09 | 2026-02-10 | sage 100% |                         |
    When the scenario is calculated
    Then the statement for "sage" includes, in order:
      | item                                          | amount    | rule   |
      | Commission before refunds                     | 2,000.00  | FR-006 |
      | Clawback: R-11 refund $60,000.00 on 2026-04-15 | 3,600.00 | FR-016 |
      | Earned commission                             | -1,600.00 | FR-015 |
    And the statement for "sage" has:
      | field                     | value      |
      | EarnedCommission          | -1,600.00  |
      | Recovered                 | 0.00       |
      | Payable                   | 0.00       |
      | ClosingRecoverableBalance | 13,600.00  |

  Scenario: US6 AS3 - a refund dated after the quarter does not affect it
    Given the quarter 2026-01-01 to 2026-03-31
    And the roster:
      | rep   | name  | quota      | start      |
      | tatum | Tatum | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit     | refunds                 |
      | R-3  | 60,000.00 | 2026-01-09 | 2026-01-10 | tatum 100% | 20,000.00 on 2026-04-20 |
      | R-4  | 60,000.00 | 2026-02-09 | 2026-02-10 | tatum 100% |                         |
    When the scenario is calculated
    Then the statement for "tatum" has no line starting "Clawback:"
    And the statement for "tatum" has:
      | field            | value    |
      | Clawbacks        | 0.00     |
      | EarnedCommission | 6,600.00 |

  Scenario: US6 AS4 - a partial refund claws back only the refunded revenue
    Given the quarter 2026-01-01 to 2026-03-31
    And the roster:
      | rep | name | quota      | start      |
      | uma | Uma  | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit   | refunds                 |
      | R-5  | 60,000.00 | 2026-01-09 | 2026-01-10 | uma 100% | 30,000.00 on 2026-03-01 |
      | R-6  | 60,000.00 | 2026-02-09 | 2026-02-10 | uma 100% |                         |
    When the scenario is calculated
    Then the statement for "uma" includes, in order:
      | item                                          | amount   | rule   |
      | Clawback: R-5 refund $30,000.00 on 2026-03-01 | 2,100.00 | FR-016 |
      | Earned commission                             | 4,500.00 | FR-015 |

  Scenario: US6 AS5 - repeated refunds are sized in date order, quarter by quarter
    Given the quarter 2026-01-01 to 2026-03-31
    And the roster:
      | rep | name | quota      | start      |
      | val | Val  | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit   | refunds                                          |
      | R-7  | 60,000.00 | 2026-01-09 | 2026-01-10 | val 100% | 20,000.00 on 2026-02-20; 20,000.00 on 2026-05-05 |
      | R-8  | 60,000.00 | 2026-02-09 | 2026-02-10 | val 100% |                                                  |
    When the scenario is calculated
    Then the statement for "val" includes, in order:
      | item                                          | amount   | rule   |
      | Clawback: R-7 refund $20,000.00 on 2026-02-20 | 1,600.00 | FR-016 |
      | Earned commission                             | 5,000.00 | FR-015 |
    And the statement for "val" has no line starting "Clawback: R-7 refund $20,000.00 on 2026-05-05"

  Scenario: US6 AS6 - two deals refunded claw back exactly what was earned
    Given the quarter 2026-01-01 to 2026-03-31
    And the roster:
      | rep  | name | quota      | start      |
      | wren | Wren | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit    | refunds                 |
      | R-9  | 60,000.00 | 2026-01-09 | 2026-01-10 | wren 100% | 60,000.00 on 2026-02-20 |
      | R-10 | 60,000.00 | 2026-01-12 | 2026-01-13 | wren 100% | 60,000.00 on 2026-03-10 |
    When the scenario is calculated
    Then the statement for "wren" includes, in order:
      | item                                           | amount   | rule   |
      | Clawback: R-9 refund $60,000.00 on 2026-02-20  | 3,600.00 | FR-016 |
      | Clawback: R-10 refund $60,000.00 on 2026-03-10 | 3,000.00 | FR-016 |
      | Clawbacks                                      | 6,600.00 | FR-016 |
      | Earned commission                              | 0.00     | FR-015 |

  Scenario: US6 AS7 - a refunded split deal is re-split before its clawback
    Given the quarter 2026-01-01 to 2026-03-31
    And the roster:
      | rep  | name | quota      | start      |
      | xan  | Xan  | 100,000.00 | 2025-06-01 |
      | yael | Yael | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount    | close      | booking    | credit             | refunds              |
      | X-1  | 10,000.20 | 2026-01-06 | 2026-01-07 | xan 50%, yael 50%  | 100.01 on 2026-02-10 |
    When the scenario is calculated
    Then the statement for "xan" includes, in order:
      | item                                                       | amount   | rule   |
      | Commission before refunds                                  | 250.01   | FR-006 |
      | X-1 re-split after refund on 2026-02-10: share of $9,900.19 | 4,950.10 | FR-017 |
      | Clawback: X-1 refund $100.01 on 2026-02-10                 | 2.50     | FR-016 |
    And the statement for "yael" includes, in order:
      | item                                                       | amount   | rule   |
      | X-1 re-split after refund on 2026-02-10: share of $9,900.19 | 4,950.09 | FR-017 |
      | Clawback: X-1 refund $100.01 on 2026-02-10                 | 2.51     | FR-016 |

  Scenario: US6 AS8 - a departed split partner stays in the booking quarter's data
    Given the quarter 2026-04-01 to 2026-06-30
    And the roster:
      | rep  | name | quota      | start      |
      | zion | Zion | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount | close | booking | credit | refunds |
    And the booking quarter 2026-01-01 to 2026-03-31 with reps:
      | rep  | quota      | start      |
      | zion | 100,000.00 | 2025-06-01 |
    And its partners:
      | rep  | start      |
      | yves | 2025-06-01 |
    And its deals:
      | deal | amount    | close      | booking    | credit                | refunds                 |
      | D1   | 60,000.00 | 2026-01-14 | 2026-01-15 | zion 100%             | 60,000.00 on 2026-04-20 |
      | D2   | 40,000.00 | 2026-02-24 | 2026-02-25 | zion 50%, yves 50%    |                         |
    When the scenario is calculated
    Then the statement for "zion" includes, in order:
      | item                                         | amount    | rule   |
      | Clawback: D1 refund $60,000.00 on 2026-04-20 | 3,000.00  | FR-016 |
      | Earned commission                            | -3,000.00 | FR-015 |
    And the statement list holds only "zion"

  Scenario: US6 AS9 - a re-split that raises a share gives a negative clawback
    Given the quarter 2026-01-01 to 2026-03-31
    And the roster:
      | rep | name | quota      | start      |
      | ari | Ari  | 100,000.00 | 2025-06-01 |
      | bo  | Bo   | 100,000.00 | 2025-06-01 |
      | cy  | Cy   | 100,000.00 | 2025-06-01 |
    And the deals:
      | deal | amount | close      | booking    | credit                      | refunds            |
      | X-2  | 10.09  | 2026-01-06 | 2026-01-07 | cy 100%                     |                    |
      | X-3  | 0.06   | 2026-01-08 | 2026-01-09 | ari 45%, bo 45%, cy 10%     | 0.01 on 2026-02-11 |
    When the scenario is calculated
    Then the statement for "cy" includes, in order:
      | item                                                  | amount | rule   |
      | Commission before refunds                             | 0.50   | FR-006 |
      | X-3 re-split after refund on 2026-02-11: share of $0.05 | 0.01   | FR-017 |
      | Clawback: X-3 refund $0.01 on 2026-02-11              | -0.01  | FR-016 |
      | Earned commission                                     | 0.51   | FR-015 |
