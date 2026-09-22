@FR-020
Feature: The application serves its page
  The calculator runs locally as a single web application with no external services.

  Scenario: The app serves its page
    Given the application is running
    When the home page is requested
    Then the response status is 200
    And the page language is "en"
    And the page has exactly one main landmark
