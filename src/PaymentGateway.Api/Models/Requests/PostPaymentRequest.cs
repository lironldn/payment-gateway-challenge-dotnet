using System.Text.Json.Serialization;

using PaymentGateway.Api.Extensions;

namespace PaymentGateway.Api.Models.Requests;

public record PostPaymentRequest
{
    [JsonPropertyName( "card_number")]
    public required string CardNumber { get; set; }

    [JsonPropertyName("expiry_date")]
    public required string ExpiryDate { get; set; }

    public int ExpiryMonth => ExpiryDate.ExpiryDateMonth();
    public int ExpiryYear => ExpiryDate.ExpiryDateYear();
    
    [JsonPropertyName("currency")]
    public required string Currency { get; set; }
    
    [JsonPropertyName("amount")]
    public required int Amount { get; set; }
    
    [JsonPropertyName("cvv")]
    public required string Cvv { get; set; }
}
