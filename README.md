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

### 1. Start the bank simulator

```bash
docker-compose up
```

This starts Mountebank on port 2525 (admin) and the bank imposter on port 8080.

### 2. Run the API

```bash
dotnet run --project src/PaymentGateway.Api
```

Swagger UI is available at `https://localhost:{port}/swagger` once running.

### 3. Run the tests

```bash
dotnet test
```

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

imposters/              # Bank simulator configuration (Mountebank)
docker-compose.yml      # Starts the bank simulator
```