using RentConnect.Data.Dtos;
using RentConnect.Data.Models;

namespace RentConnect.Data.Repositories;

public interface IListingRepository
{
    Task<List<ListingDto>> GetAllAsync(
        string? region = null,
        ListingStatus? status = null,
        string? ownerId = null,
        bool? disabledByAdmin = null,
        PropertyType? propertyType = null,
        bool? hasYard = null,
        bool? hasBalcony = null,
        bool? hasElevator = null,
        bool? hasGarage = null);
    Task<ListingDto?> GetByIdAsync(Guid id);

    // العمليات التالية تُجهّز التغيير بالذاكرة فقط - لا تُحفظ فعلياً
    // إلا بعد استدعاء IUnitOfWork.CompleteAsync() من المستهلك (Api / Web)
    Task<ListingDto> AddAsync(ListingCreateDto dto);
    Task<bool> UpdateAsync(Guid id, ListingUpdateDto dto);
    Task<ListingDto?> UpdateStatusAsync(Guid id, ListingStatus newStatus);
    Task<bool> SetDisabledByAdminAsync(Guid id, bool disabled);
    Task<bool> DeleteAsync(Guid id);

    /// <summary>ينقل ملكية الإعلان لمستخدم آخر (مثلاً بعد ما الأدمن ينشئه نيابة عن صاحب العقار) - يحدّث رقم التواصل أيضاً ليطابق المالك الجديد.</summary>
    Task<bool> TransferOwnershipAsync(Guid id, string newOwnerId, string newOwnerPhone);

    Task<bool> AddImagesAsync(Guid listingId, List<string> imageUrls);
    Task<bool> RemoveImageAsync(Guid listingId, Guid imageId);

    Task<bool> AddContractDocumentsAsync(Guid listingId, List<ContractDocumentUpload> documents);
    Task<bool> RemoveContractDocumentAsync(Guid listingId, Guid documentId);

    Task<bool> IncrementViewCountAsync(Guid id);
    Task<bool> IncrementPhoneRevealCountAsync(Guid id);

    Task<List<ListingCommentDto>> GetCommentsAsync(Guid listingId);
    Task<bool> AddCommentAsync(Guid listingId, string authorId, string authorDisplayName, string content);
    Task<bool> DeleteCommentAsync(Guid commentId);

    Task<List<ListingDto>> GetStaleAsync(int days = 7, string? ownerId = null);

    /// <summary>
    /// الإعلانات المتوفرة اللي مضى على آخر تأكيد لتوفرها أكتر من intervalDays، وما عندها
    /// سؤال واتساب معلّق (لسا ما انجاوب) - تُستخدم من خدمة التحقق الدوري بالخلفية.
    /// </summary>
    Task<List<ListingDto>> GetDueForAvailabilityCheckAsync(int intervalDays);

    /// <summary>يحدّث تاريخ آخر تأكيد لتوفر الإعلان لـ"الآن" بدون تغيير حالته أو تسجيل تاريخ حالة جديد.</summary>
    Task<bool> ConfirmStillAvailableAsync(Guid id);
}
