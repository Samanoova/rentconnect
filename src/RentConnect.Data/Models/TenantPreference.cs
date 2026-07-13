namespace RentConnect.Data.Models;

public enum TenantPreference
{
    Any = 0,               // بدون شروط
    FamiliesOnly = 1,      // عائلات فقط
    NewlywedsOnly = 2,     // عرسان فقط
    FemaleStudentsOnly = 3, // طالبات فقط
    FemalesOnly = 4         // إناث فقط
}
