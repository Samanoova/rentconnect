namespace RentConnect.Data.Models;

internal class Listing
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public PropertyType PropertyType { get; set; } = PropertyType.Apartment;

    public decimal PriceJod { get; set; }
    public string Region { get; set; } = string.Empty;   // مثلاً: عبدون، إربد، الزرقاء

    // عنوان دقيق لا يُعرض للمستخدم العام إلا بعد الدفع
    public string? PreciseAddress { get; set; }

    // إحداثيات الموقع المحدّدة من الخريطة (اختياري - لدقة أعلى من اسم المنطقة فقط)
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal? AreaSqm { get; set; }
    public int? Floor { get; set; }
    public bool IsFurnished { get; set; }

    // المرافق
    public bool HasYard { get; set; }       // حوش
    public bool HasBalcony { get; set; }    // بلكونة
    public bool HasElevator { get; set; }   // مصعد
    public bool HasGarage { get; set; }     // كراج

    // المياه
    public WaterMeterType WaterMeterType { get; set; } = WaterMeterType.Separate;
    public int? WaterCubicMeters { get; set; }   // سعة خزان المياه (متر مكعب)

    // الكهرباء
    public ElectricityMeterType ElectricityMeterType { get; set; } = ElectricityMeterType.Separate;
    public bool IsElectricitySubsidized { get; set; }   // مدعومة (تعرفة حكومية) أو لا

    // رقم تواصل المالك - يُكشف فقط بعد الدفع (لاحقاً)
    public string OwnerPhone { get; set; } = string.Empty;

    // شروط صاحب العقار
    public TenantPreference TenantPreference { get; set; } = TenantPreference.Any;
    public bool RequiresEmployedTenant { get; set; }     // شرط منفصل: يشترط أن يكون المستأجر موظفاً

    // كل كام شهر بينُدفع الإيجار (1 = شهرياً، 3 = كل 3 شهور، 12 = سنوياً... رقم حر)
    public int PaymentIntervalMonths { get; set; } = 1;

    // عقد الإيجار
    public SecurityGuaranteeType SecurityGuarantee { get; set; } = SecurityGuaranteeType.None;
    public decimal? SecurityDepositJod { get; set; }        // مبلغ التأمين - فقط لو SecurityGuarantee = SecurityDeposit
    public bool HasRentalContract { get; set; }             // يوجد عقد إيجار رسمي

    public ListingStatus Status { get; set; } = ListingStatus.Available;

    // إحصائيات التفاعل
    public int ViewCount { get; set; }          // عدد مرات مشاهدة صفحة الإعلان
    public int PhoneRevealCount { get; set; }   // عدد مرات الضغط على "إظهار الرقم"

    // معرّف صاحب الإعلان - Nullable لأن إعلانات ما قبل نظام الحسابات لا تملك صاحباً
    public string? OwnerId { get; set; }

    // تعطيل من قِبل الأدمن - مستقل عن Status الذي يتحكم به صاحب الإعلان نفسه
    public bool IsDisabledByAdmin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // آخر مرة تم تأكيد توفر العقار فعلياً
    public DateTime LastConfirmedAt { get; set; } = DateTime.UtcNow;

    // رابط المصدر الأصلي للإعلان (اختياري) - مثلاً رابط منشور فيسبوك نُقل منه الإعلان يدوياً
    public string? SourceUrl { get; set; }

    public List<ListingImage> Images { get; set; } = new();
    public List<ListingContractDocument> ContractDocuments { get; set; } = new();
    public List<ListingStatusHistory> StatusHistory { get; set; } = new();
    public List<ListingComment> Comments { get; set; } = new();
}
