@FR-021 @FR-015
Feature: A negative amount carries a minus sign
  An amount owed rather than earned is shown with a minus sign in the text, never by colour alone
  (FR-021). The data is Appendix A.10's.

  Scenario: A rep whose clawback exceeds the quarter's commission
    Given a scenario folder holding "refunds-q2.json"
    When the page for scenario "refunds-q2" is requested
    Then the summary for "Sage" shows earned commission "−$1,600.00"
