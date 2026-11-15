namespace PaymentGateway.Api.Extensions;

public static class StringExtensions
{
    public static string MaskAllButLastFour(this string value) =>
        value.Length <= 4 ? value : new string('*', value.Length - 4) + value[^4..];

    public static string LastFour(this string value) => value.Length <= 4 ? value : value[^4..];
}