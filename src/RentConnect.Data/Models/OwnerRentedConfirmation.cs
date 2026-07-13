namespace RentConnect.Data.Models;

// سؤال يُرسل لصاحب العقار عبر واتساب لما مستخدم يلغي رسم كشف رقم بحجّة إن العقار مؤجر -
// يُنتظر رد "نعم/لا" منه، ولو أجاب "نعم" يتحوّل الإعلان تلقائياً لحالة "مؤجّر"
internal class OwnerRentedConfirmation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ListingId { get; set; }
    public Guid ChargeId { get; set; }

    // رقم صاحب العقار بصيغة دولية (بدون + أو أصفار بادئة) - نطابق ردوده عليه
    public string OwnerPhone { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? AnsweredAt { get; set; }
    public bool? Answer { get; set; } // true = نعم (تأجّر)، false = لا
}
