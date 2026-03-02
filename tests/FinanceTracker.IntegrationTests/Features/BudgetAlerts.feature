Feature: Budget Alert Notifications
  As a user tracking my finances
  I want to be alerted when I exceed my budget
  So that I can control my spending

  Scenario: User exceeds monthly budget
    Given a user has set a Food budget of 5000 INR for February 2026
    And the user has already spent 4000 INR on Food
    When the user adds a Food transaction of 1500 INR
    Then a budget exceeded alert should be published
    And the budget status should show 110% usage

  Scenario: User within budget limit
    Given a user has set a Food budget of 5000 INR for February 2026
    When the user adds a Food transaction of 2000 INR
    Then no budget alert should be published
    And the budget status should show 40% usage

