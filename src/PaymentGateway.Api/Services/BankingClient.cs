using System.Net.Http.Json;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public class BankingClient(HttpClient httpClient) : IBankingClient
{
    public async Task<AuthorisePaymentResponse> AuthorizePaymentAsync(AuthorisePaymentRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("payments", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthorisePaymentResponse>()
               ?? throw new InvalidOperationException("Bank returned an empty response.");
    }
}