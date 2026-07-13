namespace RentConnect.Data.Models;

// صف واحد فقط (Id ثابت = 1) يحمل إعدادات المنصّة العامة القابلة للتعديل من لوحة الأدمن
internal class SiteSetting
{
    public int Id { get; set; } = 1;

    // المبلغ اللي بينضاف على فاتورة المستخدم في كل مرة يكشف فيها رقم تواصل جديد (بالدينار)
    public decimal PhoneRevealFeeJod { get; set; }

    // معرّف/رقم CliQ لتحويل المستحقات
    public string? CliqAlias { get; set; }

    // اسم صاحب حساب CliQ - يظهر للمستخدم للتأكد إنه بيحوّل للشخص الصح
    public string? CliqAccountName { get; set; }

    // إعدادات Evolution API (واتساب) - تُستخدم لإرسال رمز التحقق (OTP) عند إنشاء حساب جديد
    public string? EvolutionApiBaseUrl { get; set; }
    public string? EvolutionApiKey { get; set; }

    // اسم الـ instance المتصلة اللي رح تُستخدم لإرسال رسائل التحقق
    public string? EvolutionApiInstanceName { get; set; }

    // خدمة التحقق الدوري من توفر الإعلانات عبر واتساب - تفعيل/تعطيل وكل كم يوم
    public bool AvailabilityCheckEnabled { get; set; }
    public int? AvailabilityCheckIntervalDays { get; set; }
}
