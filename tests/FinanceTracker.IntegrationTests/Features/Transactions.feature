Feature: Transactions API
  As a user tracking my finances
  I want to record and view my transactions
  So that I can understand my cash flow

  Scenario: User creates a valid expense transaction
    Given a user exists in the system
    When the user creates an Expense transaction of 1000 INR in February 2026 for Food
    Then the transaction should be created successfully
    And the monthly transactions should include that transaction

  Scenario: User attempts to create an invalid transaction
    Given a user exists in the system
    When the user creates an Expense transaction of -500 INR in February 2026 for Food
    Then the transaction request should be rejected as bad request

