using RentConnect.Data.Dtos;
using RentConnect.Data.UnitOfWork;

namespace RentConnect.Web.Services;

public interface IListingAvailabilityChecker
{
    /// <summary>
    /// يرسل سؤال واتساب لصاحب الإعلان (هل تم تأجيره؟) وينشئ سجل انتظار للرد.
    /// chargeId اختياري - يُمرَّر Guid.Empty لو السؤال مش مرتبط برسم كشف رقم معيّن
    /// (مثلاً تحقّق يدوي من الأدمن أو تحقّق دوري تلقائي).
    /// </summary>
    Task<(bool Success, string? Error)> TriggerCheckAsync(ListingDto listing, Guid chargeId, string message);
}

internal class ListingAvailabilityChecker : IListingAvailabilityChecker
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEvolutionApiClient _evoClient;

    public ListingAvailabilityChecker(IUnitOfWork unitOfWork, IEvolutionApiClient evoClient)
    {
        _unitOfWork = unitOfWork;
        _evoClient = evoClient;
    }

    public async Task<(bool Success, string? Error)> TriggerCheckAsync(ListingDto listing, Guid chargeId, string message)
    {
        if (string.IsNullOrWhiteSpace(listing.OwnerPhone))
        {
            return (false, $"لا يوجد رقم تواصل صالح لإعلان \"{listing.Title}\".");
        }

        var settings = await _unitOfWork.Settings.GetAsync();
        if (string.IsNullOrWhiteSpace(settings.EvolutionApiBaseUrl) ||
            string.IsNullOrWhiteSpace(settings.EvolutionApiKey) ||
            string.IsNullOrWhiteSpace(settings.EvolutionApiInstanceName))
        {
            return (false, "لم يتم إعداد واتساب (Evolution API) بعد - أضف الإعدادات من صفحة \"الإعدادات\" أولاً.");
        }

        var whatsAppNumber = WhatsAppPhoneFormatter.ToInternational(listing.OwnerPhone);

        await _unitOfWork.OwnerConfirmations.CreateAsync(listing.Id, chargeId, whatsAppNumber);
        await _unitOfWork.CompleteAsync();

        var (success, error) = await _evoClient.SendTextMessageAsync(
            settings.EvolutionApiBaseUrl!, settings.EvolutionApiKey!, settings.EvolutionApiInstanceName!,
            whatsAppNumber, message);

        return success ? (true, null) : (false, $"تعذّر إرسال الرسالة: {error}");
    }
}
