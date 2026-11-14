using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentRepositoryTests
{
    private static PaymentsRepository CreateSut() =>
        new(Substitute.For<ILogger<PaymentsRepository>>());
    [Fact]
    public void GetPayment_ReturnsPayment()
    {
        // Arrange
        var sut = CreateSut();
        var id = Guid.NewGuid();
        sut.Add(new PostPaymentResponse
        {
            Id = id,
            Amount = 1000,
            Currency = "GBP",
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = "1234",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.Today.Year + 1
        });
        
        // Act
        var result = sut.Get(id);
        
        // Assert
        result.Should().NotBeNull();       
        result.Id.Should().Be(id);
    }

    [Fact]
    public void GetPaymentForInvalidId_ReturnsNull()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = sut.Get(Guid.NewGuid());
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void AddNewPayment_Succeeds()
    {
        // Arrange
        var sut = CreateSut();
        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Amount = 1000,
            Currency = "GBP",
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = "1234",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.Today.Year + 1
        };
        
        // Act
        sut.Add(payment);
        
        // Assert
        sut.Get(payment.Id).Should().NotBeNull();      
    }

    [Fact]
    public void AddWithClashingId_Throws()
    {
        // Arrange
        var sut = CreateSut();
        var payment1 = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Amount = 1000,
            Currency = "GBP",
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = "1234",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.Today.Year + 1
        };
        var payment2 = new PostPaymentResponse
        {
            Id = payment1.Id,
            Amount = 2000,
            Currency = "USD",
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = "5678",
            ExpiryMonth = 7,
            ExpiryYear = DateTime.Today.Year + 1
        };
        
        sut.Add(payment1);
        
        // Act
        Action act = () => sut.Add(payment2);
        act.Should().Throw<ConflictException>()
            .WithMessage($"Payment with id '{payment1.Id}' already exists");      
    }
}