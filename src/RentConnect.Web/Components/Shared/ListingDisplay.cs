using RentConnect.Data.Dtos;
using RentConnect.Data.Models;

namespace RentConnect.Web.Components.Shared;

internal static class ListingDisplay
{
    public static string PaymentFrequencyLabel(int intervalMonths) => intervalMonths switch
    {
        1 => "شهرياً",
        2 => "كل شهرين",
        3 => "كل 3 شهور",
        6 => "كل 6 شهور",
        12 => "سنوياً",
        <= 0 => "شهرياً",
        _ => $"كل {intervalMonths} شهر",
    };

    public static string? TenantPreferenceLabel(TenantPreference preference) => preference switch
    {
        TenantPreference.FamiliesOnly => "عائلات فقط",
        TenantPreference.NewlywedsOnly => "عرسان فقط",
        TenantPreference.FemaleStudentsOnly => "طالبات فقط",
        TenantPreference.FemalesOnly => "إناث فقط",
        _ => null,
    };

    // شروط صاحب العقار مجتمعة - تفضيل المستأجر (إن وُجد) + شرط الموظفين المنفصل
    public static IEnumerable<string> ConditionLabels(ListingDto listing)
    {
        var preference = TenantPreferenceLabel(listing.TenantPreference);
        if (preference is not null) yield return preference;
        if (listing.RequiresEmployedTenant) yield return "موظفين فقط";
    }

    public static string PropertyTypeLabel(PropertyType type) => type switch
    {
        PropertyType.Studio => "استديو",
        PropertyType.Roof => "روف",
        PropertyType.Villa => "فيلا",
        _ => "شقة",
    };

    public static IEnumerable<string> AmenityLabels(ListingDto listing)
    {
        if (listing.HasYard) yield return "🌳 حوش";
        if (listing.HasBalcony) yield return "🏗 بلكونة";
        if (listing.HasElevator) yield return "🛗 مصعد";
        if (listing.HasGarage) yield return "🚗 كراج";
    }

    public static string WaterMeterTypeLabel(WaterMeterType type) =>
        type == WaterMeterType.Shared ? "مشتركة" : "منفصلة";

    public static string ElectricityMeterTypeLabel(ElectricityMeterType type) =>
        type == ElectricityMeterType.Shared ? "مشتركة" : "منفصلة";

    public static string? SecurityGuaranteeLabel(SecurityGuaranteeType type) => type switch
    {
        SecurityGuaranteeType.PromissoryNote => "مطلوب توقيع كمبيالة",
        SecurityGuaranteeType.SecurityDeposit => "مطلوب مبلغ تأمين",
        _ => null,
    };

    public static bool IsImageFile(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp";
    }

    public static bool IsVideoFile(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext is ".mp4" or ".webm" or ".mov" or ".ogg" or ".m4v";
    }
}
