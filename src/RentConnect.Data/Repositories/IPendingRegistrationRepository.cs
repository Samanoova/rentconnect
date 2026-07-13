using RentConnect.Data.Dtos;

namespace RentConnect.Data.Repositories;

public interface IPendingRegistrationRepository
{
    /// <summary>يحذف أي طلب تسجيل معلّق سابق لنفس البريد أو رقم الهاتف قبل إنشاء طلب جديد.</summary>
    Task RemoveExistingForAsync(string email, string phoneNumber);

    Task<Guid> CreateAsync(string email, string phoneNumber, string? occupation, string passwordHash, string otpCode, TimeSpan validFor);

    Task<PendingRegistrationDto?> GetAsync(Guid id);

    Task<(OtpVerifyResult Result, PendingRegistrationDto? Verified)> VerifyAsync(Guid id, string code);

    /// <summary>يولّد رمز تحقق جديد ويرجّع تفاصيل الطلب لإعادة الإرسال - null لو الطلب غير موجود أو لسا داخل فترة الانتظار.</summary>
    Task<PendingRegistrationDto?> RegenerateCodeAsync(Guid id, string newCode, TimeSpan validFor, TimeSpan resendCooldown);

    Task RemoveAsync(Guid id);
}
