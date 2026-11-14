using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository
{
    public Dictionary<Guid,PostPaymentResponse> Payments = new();
    
    public void Add(PostPaymentResponse payment)
    {
        if (!Payments.TryAdd(payment.Id, payment))
            throw new ArgumentException("Payment with that ID already exists");
    }

    public PostPaymentResponse? Get(Guid id)
    {
        return Payments.GetValueOrDefault(id);
    }
}