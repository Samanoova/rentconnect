namespace RentConnect.Data.Models;

// طلب تسجيل حساب لسا ما تأكّد رقم هاتفه عبر واتساب - الحساب الفعلي ما ينحفظ بجدول
// المستخدمين إلا بعد إدخال رمز التحقق الصحيح؛ يُحذف هذا الصف فوراً بعد النجاح أو انتهاء صلاحيته
internal class PendingRegistration
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Occupation { get; set; }

    // كلمة المرور مُشفّرة مسبقاً (نفس آلية تشفير ASP.NET Core Identity) - ما بنخزّن نص صريح أبداً
    public string PasswordHash { get; set; } = string.Empty;

    public string OtpCode { get; set; } = string.Empty;
    public int Attempts { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSentAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}
