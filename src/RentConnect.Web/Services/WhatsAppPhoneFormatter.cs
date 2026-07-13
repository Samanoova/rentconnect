namespace RentConnect.Web.Services;

// يحوّل رقم أردني مكتوب بأي شكل شائع (07XXXXXXXX، +9627XXXXXXXX، 009627XXXXXXXX)
// لصيغة واتساب الدولية اللي بتحتاجها Evolution API (962XXXXXXXXX بدون + أو أصفار بادئة)
internal static class WhatsAppPhoneFormatter
{
    public static string ToInternational(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("00962", StringComparison.Ordinal))
            return digits[2..];

        if (digits.StartsWith("962", StringComparison.Ordinal))
            return digits;

        if (digits.StartsWith("0", StringComparison.Ordinal))
            return "962" + digits[1..];

        return "962" + digits;
    }
}
