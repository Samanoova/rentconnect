using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentConnect.Data.Data;
using RentConnect.Data.Identity;
using RentConnect.Data.Models;

namespace RentConnect.Data.Extensions;

public static class DataLayerInitializer
{
    private const string SeedAdminUserName = "Admin";
    private const string SeedAdminEmail = "admin@rentconnect.local";
    private const string SeedAdminPassword = "Admin";

    private const string SeedOwner1UserName = "sample_owner1";

    /// <summary>
    /// يطبّق أي Migrations معلّقة (يعمل مع أي مزوّد - SQLite أو SQL Server)، ثم يهيّئ
    /// دور الأدمن وحساب الأدمن الافتراضي (Admin / Admin) إن لم يكونا موجودين.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var db = provider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(RoleNames.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(RoleNames.Admin));
        }

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByNameAsync(SeedAdminUserName);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = SeedAdminUserName,
                Email = SeedAdminEmail,
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(admin, SeedAdminPassword);
        }

        if (!await userManager.IsInRoleAsync(admin, RoleNames.Admin))
        {
            await userManager.AddToRoleAsync(admin, RoleNames.Admin);
        }

        // بيانات تجريبية (Demo) - تُضاف مرة وحدة فقط (نتحقق من وجود حساب المالك التجريبي الأول
        // كعلامة، بغض النظر عن وجود إعلانات حقيقية أخرى)، لتجربة الواجهة بدون إدخال إعلانات يدوياً.
        // كل الأصحاب هون حسابات وهمية مخصصة للتجربة فقط.
        if (await userManager.FindByNameAsync(SeedOwner1UserName) is null)
        {
            await SeedSampleListingsAsync(db, userManager);
        }
    }

    private static async Task<string> EnsureSeedOwnerAsync(
        UserManager<ApplicationUser> userManager, string userName, string email, string phone, string occupation)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                PhoneNumber = phone,
                Occupation = occupation,
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(user, "Owner123");
        }

        return user.Id;
    }

    private static async Task SeedSampleListingsAsync(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        var owner1Id = await EnsureSeedOwnerAsync(userManager, SeedOwner1UserName, "owner1@rentconnect.local", "0791234567", "مهندس");
        var owner2Id = await EnsureSeedOwnerAsync(userManager, "sample_owner2", "owner2@rentconnect.local", "0781234567", "معلّمة");

        var listings = new List<Listing>
        {
            new()
            {
                Title = "شقة مفروشة فاخرة في عبدون",
                Description = "شقة مفروشة بالكامل بإطلالة رائعة، قريبة من كل الخدمات والمطاعم.",
                PropertyType = PropertyType.Apartment,
                PriceJod = 450,
                Region = "عبدون، عمّان",
                Latitude = 31.9454,
                Longitude = 35.8709,
                Bedrooms = 3,
                Bathrooms = 2,
                AreaSqm = 150,
                Floor = 2,
                IsFurnished = true,
                HasBalcony = true,
                HasElevator = true,
                HasGarage = true,
                WaterMeterType = WaterMeterType.Separate,
                ElectricityMeterType = ElectricityMeterType.Separate,
                OwnerPhone = "0791234567",
                TenantPreference = TenantPreference.FamiliesOnly,
                PaymentIntervalMonths = 1,
                SecurityGuarantee = SecurityGuaranteeType.SecurityDeposit,
                SecurityDepositJod = 450,
                HasRentalContract = true,
                OwnerId = owner1Id,
            },
            new()
            {
                Title = "استديو قريب من الجامعة الأردنية",
                Description = "استديو صغير ومناسب للطالبات، بالقرب من بوابة الجامعة الأردنية مباشرة.",
                PropertyType = PropertyType.Studio,
                PriceJod = 220,
                Region = "الجبيهة، عمّان",
                Latitude = 32.0100,
                Longitude = 35.8700,
                Bedrooms = 1,
                Bathrooms = 1,
                AreaSqm = 55,
                Floor = 1,
                IsFurnished = true,
                WaterMeterType = WaterMeterType.Shared,
                ElectricityMeterType = ElectricityMeterType.Shared,
                IsElectricitySubsidized = true,
                OwnerPhone = "0781234567",
                TenantPreference = TenantPreference.FemaleStudentsOnly,
                PaymentIntervalMonths = 3,
                SecurityGuarantee = SecurityGuaranteeType.PromissoryNote,
                OwnerId = owner2Id,
            },
            new()
            {
                Title = "فيلا فخمة مع حديقة في دابوق",
                Description = "فيلا واسعة على قطعة أرض خاصة، مناسبة للعائلات الكبيرة.",
                PropertyType = PropertyType.Villa,
                PriceJod = 1200,
                Region = "دابوق، عمّان",
                Latitude = 31.9700,
                Longitude = 35.8000,
                Bedrooms = 5,
                Bathrooms = 4,
                AreaSqm = 400,
                IsFurnished = false,
                HasYard = true,
                HasGarage = true,
                WaterMeterType = WaterMeterType.Separate,
                ElectricityMeterType = ElectricityMeterType.Separate,
                OwnerPhone = "0791234567",
                TenantPreference = TenantPreference.Any,
                PaymentIntervalMonths = 6,
                SecurityGuarantee = SecurityGuaranteeType.SecurityDeposit,
                SecurityDepositJod = 1200,
                HasRentalContract = true,
                OwnerId = owner1Id,
            },
            new()
            {
                Title = "روف مع تراس في الحي الجامعي - إربد",
                Description = "روف علوي بتراس واسع وإطلالة مفتوحة، مناسب للموظفين.",
                PropertyType = PropertyType.Roof,
                PriceJod = 280,
                Region = "الحي الجامعي، إربد",
                Latitude = 32.5556,
                Longitude = 35.8500,
                Bedrooms = 2,
                Bathrooms = 1,
                AreaSqm = 120,
                Floor = 4,
                IsFurnished = false,
                HasBalcony = true,
                WaterMeterType = WaterMeterType.Separate,
                ElectricityMeterType = ElectricityMeterType.Separate,
                OwnerPhone = "0781234567",
                RequiresEmployedTenant = true,
                PaymentIntervalMonths = 1,
                SecurityGuarantee = SecurityGuaranteeType.None,
                OwnerId = owner2Id,
            },
            new()
            {
                Title = "شقة عائلية اقتصادية في الزرقاء الجديدة",
                Description = "شقة نظيفة ومناسبة للعائلات بسعر اقتصادي.",
                PropertyType = PropertyType.Apartment,
                PriceJod = 200,
                Region = "الزرقاء الجديدة",
                Latitude = 32.0728,
                Longitude = 36.0876,
                Bedrooms = 3,
                Bathrooms = 1,
                AreaSqm = 130,
                Floor = 3,
                IsFurnished = false,
                WaterMeterType = WaterMeterType.Shared,
                ElectricityMeterType = ElectricityMeterType.Separate,
                OwnerPhone = "0791234567",
                TenantPreference = TenantPreference.FamiliesOnly,
                PaymentIntervalMonths = 1,
                SecurityGuarantee = SecurityGuaranteeType.None,
                OwnerId = owner1Id,
            },
            new()
            {
                Title = "شقة مطلة على البحر في العقبة",
                Description = "شقة مفروشة قريبة من الكورنيش، مثالية للإجازات أو السكن الدائم.",
                PropertyType = PropertyType.Apartment,
                PriceJod = 350,
                Region = "العقبة",
                Latitude = 29.5320,
                Longitude = 35.0063,
                Bedrooms = 2,
                Bathrooms = 2,
                AreaSqm = 100,
                Floor = 5,
                IsFurnished = true,
                HasBalcony = true,
                HasElevator = true,
                WaterMeterType = WaterMeterType.Separate,
                ElectricityMeterType = ElectricityMeterType.Separate,
                OwnerPhone = "0781234567",
                TenantPreference = TenantPreference.Any,
                PaymentIntervalMonths = 1,
                SecurityGuarantee = SecurityGuaranteeType.SecurityDeposit,
                SecurityDepositJod = 350,
                OwnerId = owner2Id,
            },
        };

        db.Listings.AddRange(listings);
        await db.SaveChangesAsync();
    }
}
