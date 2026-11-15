Feature: Payments API scenarios

Background:
    Given the services are already running

Scenario: A customer posts an authorised payment
    When a customer submits a payment
        | CardNumber       | ExpiryMonth | ExpiryYear | Amount | Currency | Cvv |
        | 4242424242424241 |          12 |       2030 |    100 | GBP      | 123 |
    Then the response status should be 200
    And the response payment status should be "Authorized"

Scenario: A customer posts a declined payment
    When a customer submits a payment
        | CardNumber       | ExpiryMonth | ExpiryYear | Amount | Currency | Cvv |
        | 4242424242424242 |          12 |       2030 |    250 | EUR      | 123 |
    Then the response status should be 200
    And the response payment status should be "Declined"

Scenario Outline: A customer posts a rejected payment
    When a customer submits a payment
        | CardNumber       | ExpiryMonth | ExpiryYear | Amount   | Currency   | Cvv   |
        | 4242424242424241 |          12 | <Year>     | <Amount> | <Currency> | <Cvv> |
    Then the response status should be 200
    And the response payment status should be "Rejected"
    Examples:
    | Year | Amount | Currency | Cvv |
    | 2020 |    125 | EUR      | 123 |
    | 2030 |      0 | GBP      | 123 |
    | 2030 |    200 | JPY      | 123 |
    | 2030 |    300 | USD      |  12 |

Scenario: A customer tries to post a payment but the banking service is down
    When a customer submits a payment
        | CardNumber       | ExpiryMonth | ExpiryYear | Amount | Currency | Cvv |
        | 4242424242424240 |          12 |       2030 |    100 | GBP      | 123 |
    Then the response status should be 503
