using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentConnect.Data.Dtos;
using RentConnect.Data.Models;
using RentConnect.Data.UnitOfWork;
using RentConnect.Web.Services;

namespace RentConnect.Web.Endpoints;

// نقاط نهاية Minimal API عادية (وليست مكوّنات Blazor) لأن تسجيل الدخول/الخروج
// يحتاج كتابة كوكي المصادقة فعلياً على المتصفح - وهذا لا يعمل من داخل دائرة
// Blazor Server التفاعلية (SignalR) لأن الاستجابة الأصلية تكون قد انتهت مسبقاً.
// صفحات Login/Register تستخدم نموذج HTML عادي (<form>) يرسل هذه الطلبات كـ POST
// حقيقي من المتصفح مباشرة، فتُعامل كطلب HTTP تقليدي.
public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/account");

        group.MapPost("/login", async (
            HttpContext http,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager) =>
        {
            var form = await http.Request.ReadFormAsync();
            var userNameOrEmail = form["UserNameOrEmail"].ToString();
            var password = form["Password"].ToString();
            var returnUrl = form["ReturnUrl"].ToString();

            var user = await userManager.FindByNameAsync(userNameOrEmail)
                       ?? await userManager.FindByEmailAsync(userNameOrEmail);

            if (user is null || user.IsBanned)
            {
                return Results.Redirect(BuildRedirect("/login", returnUrl, user is null ? "invalid" : "banned"));
            }

            var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return Results.Redirect(BuildRedirect("/login", returnUrl, "invalid"));
            }

            return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        });

        // الخطوة 1 من التسجيل: يتحقق من صحة البيانات، يرسل رمز تحقق عبر واتساب،
        // ولا يُنشئ الحساب الفعلي إلا بعد إدخال الرمز الصحيح بـ /account/register-verify
        group.MapPost("/register-request", async (
            HttpContext http,
            UserManager<ApplicationUser> userManager,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IUnitOfWork unitOfWork,
            IEvolutionApiClient evoClient) =>
        {
            var form = await http.Request.ReadFormAsync();
            var email = form["Email"].ToString();
            var phoneNumber = form["PhoneNumber"].ToString().Trim();
            var occupation = form["Occupation"].ToString();
            var password = form["Password"].ToString();
            var confirmPassword = form["ConfirmPassword"].ToString();
            var returnUrl = form["ReturnUrl"].ToString();

            if (password != confirmPassword)
            {
                return Results.Redirect(BuildRedirect("/register", returnUrl, "mismatch"));
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Results.Redirect(BuildRedirect("/register", returnUrl, "failed"));
            }

            if (await userManager.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
            {
                return Results.Redirect(BuildRedirect("/register", returnUrl, "phone_taken"));
            }

            if (await userManager.FindByEmailAsync(email) is not null)
            {
                return Results.Redirect(BuildRedirect("/register", returnUrl, "failed"));
            }

            var settings = await unitOfWork.Settings.GetAsync();
            if (string.IsNullOrWhiteSpace(settings.EvolutionApiBaseUrl) ||
                string.IsNullOrWhiteSpace(settings.EvolutionApiKey) ||
                string.IsNullOrWhiteSpace(settings.EvolutionApiInstanceName))
            {
                return Results.Redirect(BuildRedirect("/register", returnUrl, "otp_unavailable"));
            }

            await unitOfWork.PendingRegistrations.RemoveExistingForAsync(email, phoneNumber);

            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            var passwordHash = passwordHasher.HashPassword(new ApplicationUser(), password);

            var pendingId = await unitOfWork.PendingRegistrations.CreateAsync(
                email, phoneNumber, string.IsNullOrWhiteSpace(occupation) ? null : occupation,
                passwordHash, otpCode, TimeSpan.FromMinutes(10));

            await unitOfWork.CompleteAsync();

            var whatsAppNumber = WhatsAppPhoneFormatter.ToInternational(phoneNumber);
            var (sent, _) = await evoClient.SendTextMessageAsync(
                settings.EvolutionApiBaseUrl!, settings.EvolutionApiKey!, settings.EvolutionApiInstanceName!,
                whatsAppNumber, $"رمز التحقق الخاص بك في RentConnect: {otpCode}\nصالح لمدة 10 دقائق.");

            if (!sent)
            {
                await unitOfWork.PendingRegistrations.RemoveAsync(pendingId);
                await unitOfWork.CompleteAsync();
                return Results.Redirect(BuildRedirect("/register", returnUrl, "otp_send_failed"));
            }

            var verifyUrl = $"/verify-phone?id={pendingId}";
            if (!string.IsNullOrWhiteSpace(returnUrl))
                verifyUrl += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";

            return Results.Redirect(verifyUrl);
        });

        // الخطوة 2: التحقق من الرمز - وعندها فقط يُنشأ الحساب الفعلي بجدول المستخدمين
        group.MapPost("/register-verify", async (
            HttpContext http,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUnitOfWork unitOfWork) =>
        {
            var form = await http.Request.ReadFormAsync();
            var returnUrl = form["ReturnUrl"].ToString();

            if (!Guid.TryParse(form["Id"].ToString(), out var id))
            {
                return Results.Redirect(BuildRedirect("/register", returnUrl, "failed"));
            }

            var code = form["Code"].ToString().Trim();
            var (result, verified) = await unitOfWork.PendingRegistrations.VerifyAsync(id, code);
            await unitOfWork.CompleteAsync();

            if (result == OtpVerifyResult.InvalidCode)
            {
                return Results.Redirect(BuildRedirect($"/verify-phone?id={id}", returnUrl, "otp_invalid"));
            }

            if (result != OtpVerifyResult.Success || verified is null)
            {
                var errorCode = result switch
                {
                    OtpVerifyResult.Expired => "otp_expired",
                    OtpVerifyResult.TooManyAttempts => "otp_too_many",
                    _ => "failed",
                };
                return Results.Redirect(BuildRedirect("/register", returnUrl, errorCode));
            }

            var user = new ApplicationUser
            {
                UserName = verified.Email,
                Email = verified.Email,
                PhoneNumber = verified.PhoneNumber,
                Occupation = verified.Occupation,
                PhoneNumberConfirmed = true,
            };
            user.PasswordHash = verified.PasswordHash;

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return Results.Redirect(BuildRedirect("/register", returnUrl, "failed"));
            }

            await unitOfWork.PendingRegistrations.RemoveAsync(id);
            await unitOfWork.CompleteAsync();

            await signInManager.SignInAsync(user, isPersistent: true);
            return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        });

        // إعادة إرسال رمز التحقق (بحد أقصى مرة كل 45 ثانية) لنفس طلب التسجيل المعلّق
        group.MapPost("/register-resend", async (
            HttpContext http,
            IUnitOfWork unitOfWork,
            IEvolutionApiClient evoClient) =>
        {
            var form = await http.Request.ReadFormAsync();
            var returnUrl = form["ReturnUrl"].ToString();

            if (!Guid.TryParse(form["Id"].ToString(), out var id))
            {
                return Results.Redirect(BuildRedirect("/register", returnUrl, "failed"));
            }

            var pending = await unitOfWork.PendingRegistrations.GetAsync(id);
            if (pending is null)
            {
                return Results.Redirect(BuildRedirect("/register", returnUrl, "otp_expired"));
            }

            var settings = await unitOfWork.Settings.GetAsync();
            if (string.IsNullOrWhiteSpace(settings.EvolutionApiBaseUrl) ||
                string.IsNullOrWhiteSpace(settings.EvolutionApiKey) ||
                string.IsNullOrWhiteSpace(settings.EvolutionApiInstanceName))
            {
                return Results.Redirect(BuildRedirect($"/verify-phone?id={id}", returnUrl, "otp_send_failed"));
            }

            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            var updated = await unitOfWork.PendingRegistrations.RegenerateCodeAsync(
                id, otpCode, TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(45));
            await unitOfWork.CompleteAsync();

            if (updated is null)
            {
                return Results.Redirect(BuildRedirect($"/verify-phone?id={id}", returnUrl, "otp_resend_wait"));
            }

            var whatsAppNumber = WhatsAppPhoneFormatter.ToInternational(updated.PhoneNumber);
            await evoClient.SendTextMessageAsync(
                settings.EvolutionApiBaseUrl!, settings.EvolutionApiKey!, settings.EvolutionApiInstanceName!,
                whatsAppNumber, $"رمز التحقق الخاص بك في RentConnect: {otpCode}\nصالح لمدة 10 دقائق.");

            var verifyUrl = $"/verify-phone?id={id}&status=otp_resent";
            if (!string.IsNullOrWhiteSpace(returnUrl))
                verifyUrl += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";

            return Results.Redirect(verifyUrl);
        });

        group.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Redirect("/");
        });

        group.MapGet("/login-google", (string? returnUrl, HttpContext http) =>
        {
            var callbackUrl = $"{http.Request.Scheme}://{http.Request.Host}/account/google-callback?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";
            var properties = new AuthenticationProperties { RedirectUri = callbackUrl };
            return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
        });

        group.MapGet("/google-callback", async (
            string? returnUrl,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager) =>
        {
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info is null)
            {
                return Results.Redirect(BuildRedirect("/login", returnUrl, "external"));
            }

            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                var signedInUser = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (signedInUser is { IsBanned: true })
                {
                    await signInManager.SignOutAsync();
                    return Results.Redirect(BuildRedirect("/login", returnUrl, "banned"));
                }

                return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                return Results.Redirect(BuildRedirect("/login", returnUrl, "external"));
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return Results.Redirect(BuildRedirect("/login", returnUrl, "external"));
                }
            }

            if (user.IsBanned)
            {
                return Results.Redirect(BuildRedirect("/login", returnUrl, "banned"));
            }

            await userManager.AddLoginAsync(user, info);
            await signInManager.SignInAsync(user, isPersistent: true);
            return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        });

        return endpoints;
    }

    private static string BuildRedirect(string path, string? returnUrl, string error)
    {
        // بعض المسارات (مثلاً /verify-phone?id=...) بيجي معها query string جاهز - لازم نستخدم & مش ?
        var separator = path.Contains('?') ? "&" : "?";
        var url = $"{path}{separator}error={error}";
        if (!string.IsNullOrWhiteSpace(returnUrl))
            url += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
        return url;
    }
}
