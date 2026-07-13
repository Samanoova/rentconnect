using Microsoft.AspNetCore.Components.Authorization;
using RentConnect.Data.Extensions;
using RentConnect.Web.Components;
using RentConnect.Web.Endpoints;
using RentConnect.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// نفس استدعاء تسجيل طبقة البيانات المستخدم بمشروع API بالضبط - يشمل الآن نظام الحسابات
builder.Services.AddRentConnectData(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// يُستخدم من LocationPicker لتحويل الإحداثيات لاسم منطقة (reverse geocoding) عبر Nominatim
builder.Services.AddHttpClient();

// تكامل واتساب عبر Evolution API - يُستخدم لإدارة الـ instances ولإرسال رمز التحقق عند التسجيل
builder.Services.AddHttpClient<IEvolutionApiClient, EvolutionApiClient>();

// منطق مشترك لإرسال سؤال "هل تم تأجيره؟" لصاحب الإعلان - يُستخدم من صفحة الفاتورة، إدارة
// الإعلانات، وخدمة التحقق الدوري بالخلفية
builder.Services.AddScoped<IListingAvailabilityChecker, ListingAvailabilityChecker>();

// خدمة تعمل بالخلفية تتحقق دورياً (كل ساعة) هل فيه إعلانات مستحقة لإرسال سؤال تأكيد التوفر،
// حسب الإعدادات (مفعّلة؟ وكل كم يوم؟) القابلة للتعديل من لوحة الأدمن
builder.Services.AddHostedService<AvailabilityCheckBackgroundService>();

var app = builder.Build();

await DataLayerInitializer.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapAccountEndpoints();
app.MapWebhookEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
