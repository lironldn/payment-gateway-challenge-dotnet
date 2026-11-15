# Payment Gateway Challenge (.NET)

A payment gateway API that validates payment requests and authorises them via a simulated bank.

## Architecture

```
Client → Payment Gateway API → Bank Simulator (Mountebank)
```

- **Payment Gateway API** — ASP.NET Core 8 REST API
- **Bank Simulator** — Mountebank instance running in Docker on port 8080

## API Endpoints

### `POST /api/payments`

Submits a payment for authorisation.

**Request body:**

```json
{
  "cardNumber": "1234567890123456",
  "expiryMonth": 12,
  "expiryYear": 2027,
  "currency": "USD",
  "amount": 1000,
  "cvv": "123"
}
```

**Response:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Authorized",
  "cardNumberLastFour": "3456",
  "expiryMonth": 12,
  "expiryYear": 2027,
  "currency": "USD",
  "amount": 1000
}
```

**Status values:** `Authorized`, `Declined`, `Rejected`

- `Rejected` — request failed validation (returned immediately, not forwarded to bank)
- `Declined` — bank refused the payment
- `Authorized` — bank approved the payment
- `503 Service Unavailable` — the bank returned an error, so the API could not complete the authorization

### `GET /api/payments/{id}`

Retrieves a previously processed payment by its ID.

## Validation Rules

| Field         | Rule                                           |
|---------------|------------------------------------------------|
| `cardNumber`  | 14–19 digits, no spaces or dashes              |
| `expiryMonth` | 1–12                                           |
| `expiryYear`  | Combined with month, must be in the future     |
| `currency`    | `USD`, `EUR`, or `GBP`                         |
| `amount`      | >= 1                                           |
| `cvv`         | 3–4 digits                                     |

## Running Locally

### Prerequisites

- .NET 8 SDK
- Docker (for the bank simulator)

### 1. Start the local stack

```bash
docker compose up --build
```

This starts:
- the API in its own container on `http://localhost:5100`
- the Mountebank bank simulator on `http://localhost:8080`
- the Mountebank admin UI on `http://localhost:2525`

The API container is configured to talk to the simulator using the Docker network host name `bank_simulator`, while the local appsettings file still defaults to `http://localhost:8080/` for non-container runs.

For the acceptance tests, the services are expected to be already running before the test run begins.

### 2. Run the API locally outside Docker

The banking client base URL and logging configuration are read from `src/PaymentGateway.Api/appsettings.json`:

```json
"BankingClient": {
  "BaseUrl": "http://localhost:8080/"
},
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "WriteTo": [
    { "Name": "Console" }
  ]
}
```

```bash
dotnet run --project src/PaymentGateway.Api
```

Swagger UI is available at `https://localhost:{port}/swagger` once running.

### 3. Run the tests

```bash
dotnet test
```

The acceptance tests assume the API and bank simulator are already running locally. They probe `http://localhost:5100` and validate the four core scenarios:

- authorised payment
- declined payment
- rejected payment
- banking service unavailable / 503

## Project Structure

```
src/
  PaymentGateway.Api/
    Controllers/        # PaymentsController
    Models/
      Requests/         # PostPaymentRequest, AuthorisePaymentRequest
      Responses/        # PostPaymentResponse, AuthorisePaymentResponse
    Services/           # PaymentsRepository, BankingClient
    Validators/         # PostPaymentRequestValidator (FluentValidation)
    Extensions/         # StringExtensions (card number masking)

test/
  PaymentGateway.Api.Tests/
    PaymentsControllerTests.cs
    PaymentRepositoryTests.cs
    PostPaymentRequestValidatorTests.cs
  PaymentGateway.Api.AcceptanceTests/
    Features/
      PaymentsApi.feature
    StepDefinitions/
      PaymentsApiSteps.cs
    ScenarioHooks.cs
    xunit.runner.json

imposters/              # Bank simulator configuration (Mountebank)
docker-compose.yml      # Starts the bank simulator
```

## Test Types

### Component tests

The component tests live under `test/PaymentGateway.Api.Tests` and validate controller, repository, and request validation behaviour in isolation.

### Acceptance tests

The acceptance suite lives under `test/PaymentGateway.Api.AcceptanceTests` and exercises the full API flow against the running local stack on `http://localhost:5100`.

It covers:
- authorised payment
- declined payment
- rejected payment
- bank unavailable / 503 response