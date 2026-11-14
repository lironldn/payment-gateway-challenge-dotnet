using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository(ILogger<PaymentsRepository> logger) : IPaymentsRepository
{
    private readonly Dictionary<Guid, PostPaymentResponse> _payments = new();

    public void Add(PostPaymentResponse payment)
    {
        if (!_payments.TryAdd(payment.Id, payment))
            throw new ConflictException($"Payment with ID '{payment.Id}' already exists");

        logger.LogInformation("Stored payment {PaymentId} with status {Status}", payment.Id, payment.Status);
    }

    public PostPaymentResponse? Get(Guid id)
    {
        var payment = _payments.GetValueOrDefault(id);
        if (payment is null)
            logger.LogWarning("Payment {PaymentId} not found", id);
        else
            logger.LogInformation("Retrieved payment {PaymentId}", id);
        return payment;
    }
}
