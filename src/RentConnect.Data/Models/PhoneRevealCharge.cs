namespace RentConnect.Data.Models;

// سطر فاتورة واحد - يُنشأ أول مرة يكشف فيها مستخدم رقم تواصل إعلان معيّن (مرة وحدة لكل إعلان لكل مستخدم)
internal class PhoneRevealCharge
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; } = string.Empty;

    public Guid ListingId { get; set; }

    // قيمة الرسم وقت الكشف - محفوظة كنسخة تاريخية حتى لو تغيّرت قيمة الرسم لاحقاً من إعدادات الأدمن
    public decimal AmountJod { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // تُعبّى لما الأدمن يأكّد استلام التحويل عبر CliQ ويصفّر مستحقات المستخدم
    public DateTime? SettledAt { get; set; }

    // يقدر المستخدم يلغي الرسم بنفسه (بحد أقصى محدود باليوم) لو تبيّن إن العقار كان مؤجر وقت الاتصال
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
}
