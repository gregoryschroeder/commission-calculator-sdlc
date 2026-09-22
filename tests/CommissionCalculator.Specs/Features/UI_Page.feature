@FR-001 @FR-002 @FR-003 @FR-021 @SC-002
Feature: The statement page
  The page is a view of the engine: a scenario picker, and for each rep a summary and a breakdown
  whose every line cites the FR that produced it (contracts/ui.md). Each scenario uses a scenario
  folder it creates; the shipped Scenarios folder only arrives in Phase 9.

  Scenario: The picker is a labelled GET form listing every scenario
    Given a scenario folder holding "tiers.json" and "booking-dates.json"
    When the page for scenario "tiers" is requested
    Then the picker is a GET form with a labelled select named "scenario" and a submit button
    And the picker offers the scenarios "booking-dates, tiers"

  Scenario: The page has exactly one top-level heading
    Given a scenario folder holding "tiers.json"
    When the page for scenario "tiers" is requested
    Then the page has exactly one h1

  Scenario: The skip link targets the main landmark
    Given a scenario folder holding "tiers.json"
    When the page for scenario "tiers" is requested
    Then the skip link targets the main landmark

  Scenario: One section per rep, each labelled by its own heading
    Given a scenario folder holding "tiers.json"
    When the page for scenario "tiers" is requested
    Then there are exactly 4 rep sections, each labelled by its own h2

  Scenario: Each breakdown is a captioned table with column headers
    Given a scenario folder holding "tiers.json"
    When the page for scenario "tiers" is requested
    Then every rep table has a caption and the column headers "Item, Amount, Rule"

  Scenario: Every breakdown line cites a requirement that exists in the spec
    Given a scenario folder holding "tiers.json"
    When the page for scenario "tiers" is requested
    Then the page has at least one Rule cell and every Rule cell is an FR in spec.md

  Scenario: The page title names the selected scenario
    Given a scenario folder holding "tiers.json"
    When the page for scenario "tiers" is requested
    Then the page title is "Tiered rates — Commission Calculator"

  Scenario: Each summary shows the engine's figures
    Given a scenario folder holding "tiers.json"
    When the page for scenario "tiers" is requested
    Then every rep summary equals the engine's statement for "tiers.json"
    And the summary for "Avery" shows attainment "80.00%"

  Scenario: Each breakdown table is the engine's lines, in order
    Given a scenario folder holding "tiers.json"
    When the page for scenario "tiers" is requested
    Then every rep table lists exactly the engine's lines for "tiers.json"

  Scenario: US1 AS5 - selecting a different scenario shows only that scenario
    Given a scenario folder holding "tiers.json" and "booking-dates.json"
    When the page for scenario "booking-dates" is requested
    Then the page shows the reps "Emery"
    When the page for scenario "tiers" is requested
    Then the page shows the reps "Avery, Blake, Casey, Devon"

  Scenario: With no scenario chosen, the first scenario by file name is shown
    Given a scenario folder holding "tiers.json" and "booking-dates.json"
    When the page is requested with no scenario
    Then the page shows the reps "Emery"

  Scenario: An empty scenario folder says so
    Given an empty scenario folder
    When the page is requested with no scenario
    Then the page says "No scenarios are installed"
    And the picker offers no scenarios

  Scenario: An unknown scenario is not found
    Given a scenario folder holding "tiers.json"
    When the page for scenario "nope" is requested
    Then the response status is 404
    And the picker offers the scenarios "tiers"

  Scenario: A file that fails to load is reported
    Given a scenario folder holding "tiers.json" and a malformed "broken.json"
    When the page for scenario "tiers" is requested
    Then the page reports that "broken.json" failed to load
    And the page shows the reps "Avery, Blake, Casey, Devon"
