using System.Text.RegularExpressions;

namespace PaymentGateway.Api.Extensions;

public static class StringExtensionMethods
{
    public static int ExpiryDateMonth(this string str)
    {
        if (Regex.IsMatch(str, @"\d{2}/\d{4}"))
            return int.Parse(str.Substring(0, 2));

        return 0;
    }

    public static int ExpiryDateYear(this string str)
    {
        if (Regex.IsMatch(str, @"\d{2}/\d{4}"))
            return int.Parse(str.Substring(3, 4));

        return 0;
    }
}
