using FluentAssertions;
using FluentAssertions.Execution;

using Microsoft.AspNetCore.Components.Forms;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Tests;

public class PostPaymentRequestValidatorTests
{
    private static PostPaymentRequest ValidRequest() => new()
    {
        CardNumber = "123456789012345",
        ExpiryDate = $"12/{DateTime.UtcNow.Year + 1}",
        Currency = "USD",
        Amount = 100,
        Cvv = "123"
    };

    [Fact]
    public void Validate_ValidRequest_ReturnsNoErrors()
    {
        var validator = new PostPaymentRequestValidator();
        var result = validator.Validate(ValidRequest());
        
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public void Validate_ExpiredCard_ReturnsErrors()
    {
        var validator = new PostPaymentRequestValidator();
        var request = ValidRequest();
        request.ExpiryDate = $"{DateTime.UtcNow.Month}/{DateTime.UtcNow.Year}";
        var result = validator.Validate(request);
        
        using var scope = new AssertionScope();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorMessage.Should().Be("Expiry date must be in the future");
    }
    
    [Fact]
    public void Validate_InvalidCurrency_ReturnsErrors()
    {
        var validator = new PostPaymentRequestValidator();
        var request = ValidRequest();
        request.Currency = "JPY";
        var result = validator.Validate(request);
        
        using var scope = new AssertionScope();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorMessage.Should().Be("Currency must be USD, EUR or GBP");       
    }

    [Theory]
    [InlineData("1234567890123")]
    [InlineData("123456789012345678")]
    [InlineData("123456789012345A")]
    [InlineData("1234-5678-9012-3456")]
    [InlineData("1234 5678 9012 3456")]
    public void Validate_CardNumber_ReturnsErrors(string cardNumber)
    {
        var validator = new PostPaymentRequestValidator();
        var request = ValidRequest();
        request.CardNumber = cardNumber;
        var result = validator.Validate(request);
        
        using var scope = new AssertionScope();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorMessage.Should().Be("Card number must be 14-17 digits");       
    }
    
    [Theory]
    [InlineData("00/2099", "Expiry date must have a valid month (1-12)")]
    [InlineData("12/0000", "Expiry date must be in the future")]
    [InlineData("12/203A", "Expiry date must be in the format MM/YYYY")]
    [InlineData("7/23", "Expiry date must be in the format MM/YYYY")]
    public void Validate_ExpiryDate_ReturnsErrors(string expiryDate, string errorMessage)
    {
        var validator = new PostPaymentRequestValidator();
        var request = ValidRequest();
        request.ExpiryDate = expiryDate;
        
        var result = validator.Validate(request);
        
        using var scope = new AssertionScope();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(x => x.ErrorMessage == errorMessage);      
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("123A")]
    public void Validate_Cvv_ReturnsErrors(string cvv)
    {
        var validator = new PostPaymentRequestValidator();
        var request = ValidRequest();
        request.Cvv = cvv;
        
        var result = validator.Validate(request);
        
        using var scope = new AssertionScope();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(x => x.ErrorMessage == "CVV must be 3-4 digits");     
    }
    
    [Fact]
    public void Validate_Amount_ReturnsErrors()
    {
        var validator = new PostPaymentRequestValidator();
        var request = ValidRequest();
        request.Amount = 0;
        
        var result = validator.Validate(request);
        
        using var scope = new AssertionScope();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(x => x.ErrorMessage == "'Amount' must be greater than or equal to '1'.");    
    }
}
