using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IBankingClient
{
    Task<AuthorisePaymentResponse> AuthorizePaymentAsync(AuthorisePaymentRequest request);
}