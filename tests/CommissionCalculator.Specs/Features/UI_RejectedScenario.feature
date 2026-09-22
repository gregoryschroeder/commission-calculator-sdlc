@FR-004
Feature: A rejected scenario shows its errors, not a payout
  A scenario whose data breaks a rule is shown as rejected with every reason, and no rep's
  figures are shown (FR-004). The data is Appendix A.11's.

  Scenario: The invalid scenario lists its four errors and no rep tables
    Given a scenario folder holding "invalid.json"
    When the page for scenario "invalid" is requested
    Then the page reports that the scenario was rejected with 4 errors
    And the rejection cites the requirements "FR-004, FR-011, FR-011, FR-013"
    And the page shows the reps ""
