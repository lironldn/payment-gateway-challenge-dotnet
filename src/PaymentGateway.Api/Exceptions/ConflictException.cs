namespace PaymentGateway.Api.Exceptions;

public class ConflictException(string message) : Exception(message);