@SC-001 @SC-002 @SC-003
Feature: The seeded scenarios pay what the spec says
  Every shipped scenario is computed and compared with Appendix A of the committed spec, line for
  line and field for field. The brief's eight compensation rules are each exercised by at least
  one seeded scenario, and each rule's check is shown able to fail.

  Scenario Outline: <file> matches Appendix A
    Given the shipped scenario file "<file>"
    Then every statement matches Appendix A line for line
    And every summary field matches its Appendix A line
    And every breakdown line cites a requirement that exists in spec.md

    Examples:
      | file               |
      | tiers              |
      | booking-dates      |
      | proration          |
      | proration-q2       |
      | splits             |
      | split-rounding     |
      | draw               |
      | refunds            |
      | refund-splits      |
      | refunds-q2         |

  Scenario: The invalid scenario lists exactly the errors Appendix A.11 states
    Given the shipped scenario file "invalid"
    Then it is rejected with exactly 4 errors citing "FR-004, FR-011, FR-011, FR-013"

  Scenario: Every one of the brief's eight rules is exercised by a seeded scenario
    Given all the shipped scenario files
    Then rule 1 holds
    And rule 2 holds
    And rule 3 holds
    And rule 4 holds
    And rule 5 holds
    And rule 6 holds
    And rule 7 holds
    And rule 8 holds

  Scenario Outline: The rule <rule> check fails when the scenarios carrying it are removed
    Given all the shipped scenario files except "<removed>"
    Then rule <rule> does not hold

    Examples:
      | rule | removed                             |
      | 2    | tiers, draw                         |
      | 3    | proration, proration-q2             |
      | 4    | splits, split-rounding, refund-splits |
      | 5    | draw                                |
      | 6    | refunds, refund-splits, refunds-q2  |
      | 7    | booking-dates                       |

  Scenario: The rule 1 check fails when every seeded rep has the same quota
    Given all the shipped scenario files with every quota set to $100,000.00
    Then rule 1 does not hold

  Scenario: The rule 8 check fails when a seed carries a currency field
    Given all the shipped scenario files with a currency field added to one of them
    Then rule 8 does not hold

  Scenario: The rule 8 dollar check rejects an amount that is not in dollars
    Then the dollar check rejects "1234.50"
