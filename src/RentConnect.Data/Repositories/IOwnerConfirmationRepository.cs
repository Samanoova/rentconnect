namespace RentConnect.Data.Repositories;

public interface IOwnerConfirmationRepository
{
    Task CreateAsync(Guid listingId, Guid chargeId, string ownerPhoneWhatsApp);

    /// <summary>
    /// يبحث عن أحدث سؤال لسا ما انجاوب لنفس رقم صاحب العقار (يُستخدم لمطابقة رد وارد عبر الويب هوك).
    /// يرجّع معرّف الإعلان لو لقى طلب معلّق، ويعلّمه كمجاوب مباشرة.
    /// </summary>
    Task<Guid?> AnswerLatestPendingAsync(string ownerPhoneWhatsApp, bool answer);
}
