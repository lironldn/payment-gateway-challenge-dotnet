namespace PaymentGateway.Api.Extensions;

public static class StringExtensions
{
    public static string MaskAllButLastFour(this string value) =>
        new string('*', value.Length - 4) + value[^4..];
}