using RentConnect.Data.Dtos;

namespace RentConnect.Data.Repositories;

public interface IBillingRepository
{
    /// <summary>
    /// يضيف رسم كشف رقم لمستخدم عن إعلان معيّن، فقط لو ما كان محسوب له من قبل (مرة وحدة لكل إعلان لكل مستخدم).
    /// يرجّع true لو تمت إضافة رسم جديد فعلاً.
    /// </summary>
    Task<bool> ChargeForRevealAsync(string userId, Guid listingId, decimal amountJod);

    Task<bool> HasChargedAsync(string userId, Guid listingId);

    Task<decimal> GetUnsettledTotalAsync(string userId);

    Task<Dictionary<string, decimal>> GetUnsettledTotalsAsync();

    Task<List<PhoneRevealChargeDto>> GetChargesAsync(string userId);

    /// <summary>
    /// يصفّر مستحقات المستخدم بعد ما الأدمن يأكّد استلام التحويل عبر CliQ.
    /// </summary>
    Task<bool> SettleUserAsync(string userId);

    /// <summary>عدد العناصر اللي ألغاها المستخدم اليوم (بالتوقيت العالمي) - لتطبيق حد أقصى يومي.</summary>
    Task<int> GetCancelledTodayCountAsync(string userId);

    /// <summary>
    /// يلغي رسم كشف رقم بطلب من المستخدم نفسه (مثلاً لأن العقار تبيّن إنه مؤجر) - يفشل لو الرسم
    /// مدفوع/ملغى مسبقاً، أو لو المستخدم وصل الحد الأقصى المسموح للإلغاء باليوم.
    /// </summary>
    Task<(bool Success, string? Error)> CancelChargeAsync(string userId, Guid chargeId, string reason);
}
