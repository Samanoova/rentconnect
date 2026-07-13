using System.Text.RegularExpressions;

namespace RentConnect.Data.Utilities;

internal static class TextSanitizer
{
    // أي تسلسل أرقام (يسمح بفواصل زي مسافة/شرطة/نقطة/أقواس بينها) يحتوي 7 أرقام فعلية أو أكثر
    // يُعتبر رقم هاتف محتمل ويُحذف - 7 أقل من أي رقم غرف/مساحة/طابق/سعر طبيعي ممكن يظهر بنص الإعلان
    private static readonly Regex PhoneCandidateRegex = new(
        @"\+?\d[\d\-\.\s\(\)]{5,}\d",
        RegexOptions.Compiled);

    /// <summary>
    /// يحذف أي رقم هاتف محتمل من نص حر (العنوان أو الوصف) حتى يبقى رقم التواصل الرسمي
    /// هو المكان الوحيد اللي بيظهر فيه رقم بالإعلان.
    /// </summary>
    public static string? StripPhoneNumbers(string? text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var cleaned = PhoneCandidateRegex.Replace(text, match =>
            match.Value.Count(char.IsDigit) >= 7 ? string.Empty : match.Value);

        return Regex.Replace(cleaned, @"[ \t]{2,}", " ").Trim();
    }
}
