using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentConnect.Data.Data;
using RentConnect.Data.Models;
using RentConnect.Data.UnitOfWork;

namespace RentConnect.Data.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// تسجيل طبقة البيانات بالكامل: قاعدة البيانات، IUnitOfWork، وكذلك نظام الحسابات
    /// (ASP.NET Core Identity + تسجيل الدخول بجوجل إن توفرت بياناته). المُستهلِك
    /// (API أو Web) لا يحصل على AppDbContext مباشرة - فقط IUnitOfWork وخدمات Identity
    /// العامة (UserManager/SignInManager) اللازمة لصفحات تسجيل الدخول والتسجيل.
    /// </summary>
    /// <param name="contentRootPath">
    /// مسار جذر المشروع (WebApplicationBuilder.Environment.ContentRootPath) - يُستخدم لتثبيت
    /// موقع ملف SQLite بشكل مطلق، بدل ما يعتمد على مجلد العمل الحالي وقت تشغيل dotnet run
    /// (وإلا ممكن ينشئ قاعدة بيانات فاضية مختلفة كل مرة يشتغل من مجلد مختلف).
    /// </param>
    public static IServiceCollection AddRentConnectData(this IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        var provider = configuration["DatabaseProvider"] ?? "Sqlite";

        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlServer"));
            }
            else
            {
                var connectionStringBuilder = new SqliteConnectionStringBuilder(
                    configuration.GetConnectionString("Sqlite") ?? "Data Source=rentconnect.db");

                if (!Path.IsPathRooted(connectionStringBuilder.DataSource))
                {
                    connectionStringBuilder.DataSource = Path.Combine(contentRootPath, connectionStringBuilder.DataSource);
                }

                options.UseSqlite(connectionStringBuilder.ConnectionString);
            }
        });

        services.AddScoped<IUnitOfWork, RentConnect.Data.UnitOfWork.UnitOfWork>();

        services.AddCascadingAuthenticationState();
        services.AddAuthorization();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            // نظام تسجيل مبسّط لتطبيق محلي صغير - بدون تأكيد بريد إلكتروني
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;

            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 4;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.AccessDeniedPath = "/access-denied";
        });

        var googleClientId = configuration["Authentication:Google:ClientId"];
        var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
        {
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                options.SignInScheme = IdentityConstants.ExternalScheme;
            });
        }

        return services;
    }
}
