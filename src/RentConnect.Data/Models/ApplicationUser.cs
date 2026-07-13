using Microsoft.AspNetCore.Identity;

namespace RentConnect.Data.Models;

// عام (public) - على عكس بقية الكيانات - لأن ASP.NET Core Identity (UserManager,
// SignInManager) تُستخدم مباشرة من طبقة الواجهة (Web) لصفحات تسجيل الدخول والتسجيل.
public class ApplicationUser : IdentityUser
{
    public bool IsBanned { get; set; }

    // المهنة/طبيعة العمل - يُطلب عند التسجيل لأن بعض الملّاك يشترطون التأجير لموظفين فقط
    public string? Occupation { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
