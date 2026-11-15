using System.Net;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Extensions;
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
    PaymentsRepository paymentsRepository,
    ILogger<PaymentsController> logger) : Controller
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        logger.LogInformation("Getting payment with ID {id}", id);

        // in a real-world app, the calls to the repository would be async calls;
        var payment = await Task.FromResult(paymentsRepository.Get(id));
        if (payment == null)
        {
            logger.LogWarning("Payment with ID {id} not found", id);
            return NotFound();
        }

        return await Task.FromResult(new OkObjectResult(payment));
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync([FromBody] PostPaymentRequest request)
    {
        logger.LogInformation("Processing payment for {amount} {currency} to {cardNumber}", request.Amount,
            request.Currency, request.CardNumber.MaskAllButLastFour());
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            logger.LogInformation("Payment validation failed: {errors}", validation.Errors);
            return new BadRequestObjectResult(new PostPaymentResponse
            {
                Id = Guid.Empty,
                Status = PaymentStatus.Rejected,
                CardNumberLastFour = request.CardNumber.LastFour(),
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

        try
        {
            var bankingResponse = await bankingClient.AuthorizePaymentAsync(authorisePaymentRequest);
            logger.LogInformation("Banking response: {response}", bankingResponse);

            var response = new PostPaymentResponse
            {
                Id = Guid.NewGuid(),
                Status = bankingResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
                CardNumberLastFour = request.CardNumber.LastFour(),
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount
            };

            paymentsRepository.Add(response);
            return new OkObjectResult(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Banking call failed");
            return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
        }
    }
}
