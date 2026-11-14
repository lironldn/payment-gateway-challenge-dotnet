using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(
    IValidator<PostPaymentRequest> validator,
    IBankingClient bankingClient,
    PaymentsRepository paymentsRepository) : Controller
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = paymentsRepository.Get(id);
        if (payment == null)
            return NotFound();

        return new OkObjectResult(payment);
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync([FromBody] PostPaymentRequest request)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return new OkObjectResult(new PostPaymentResponse
            {
                Id = Guid.Empty,
                Status = PaymentStatus.Rejected,
                CardNumberLastFour = request.CardNumber[^4..],
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount
            });
        }

        var authorisePaymentRequest = new AuthorisePaymentRequest
        {
            CardNumber = request.CardNumber,
            Cvv = request.Cvv,
            Currency = request.Currency,
            ExpiryDate = $"{request.ExpiryMonth:00}/{request.ExpiryYear:0000}",
            Amount = request.Amount
        };
        
        var bankingResponse = await bankingClient.AuthorizePaymentAsync(authorisePaymentRequest);
        if (!bankingResponse.Authorized)
        {
            return new OkObjectResult(new PostPaymentResponse
            {
                Id = Guid.Empty,
                Status = PaymentStatus.Declined,
                CardNumberLastFour = request.CardNumber[^4..],
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount
            });
        }

        var response = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = request.CardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };
        paymentsRepository.Add(response);
        return new OkObjectResult(response);       
    }
}