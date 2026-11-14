using FluentValidation;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators;

public class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
    private static readonly HashSet<string> ValidCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD", "EUR", "GBP"
    };

    public PostPaymentRequestValidator()
    {
        RuleFor(x => x.CardNumber).Matches("^\\d{14,17}$")
            .WithMessage("Card number must be 14-17 digits");
        
        RuleFor(x => x.ExpiryMonth).InclusiveBetween(1,12)
            .WithMessage("Expiry month must be between 1 and 12");
        
        RuleFor(x => $"{x.ExpiryYear:0000}-{x.ExpiryMonth:00}")
            .GreaterThan($"{DateTime.Today.Year:0000}-{DateTime.Today.Month:00}")
            .WithMessage("Expiry date must be in the future");
        
        RuleFor(x => x.Currency).Must(x => ValidCurrencies.Contains(x))
            .WithMessage("Currency must be USD, EUR or GBP");
        
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(1);
        
        RuleFor(x => x.Cvv).Matches("^\\d{3,4}$")
            .WithMessage("CVV must be 3-4 digits");
    }
}
