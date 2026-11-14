using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository
{
    private readonly Dictionary<Guid,PostPaymentResponse> _payments = new();
    
    public void Add(PostPaymentResponse payment)
    {
        if (!_payments.TryAdd(payment.Id, payment))
            throw new ConflictException($"Payment with ID '{payment.Id}' already exists");
    }

    public PostPaymentResponse? Get(Guid id)
    {
        return _payments.GetValueOrDefault(id);
    }
}