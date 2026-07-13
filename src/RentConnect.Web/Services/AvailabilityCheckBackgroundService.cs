using RentConnect.Data.UnitOfWork;

namespace RentConnect.Web.Services;

// تعمل بالخلفية طول عمر التطبيق - كل ساعة بتتحقق هل خدمة التحقق الدوري مفعّلة من الإعدادات،
// وإذا نعم بترسل سؤال واتساب "هل تم تأجيره؟" لكل إعلان متوفر مضى على آخر تأكيد لتوفره أكتر
// من المدة المحدّدة، وما عنده سؤال معلّق أصلاً.
internal class AvailabilityCheckBackgroundService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AvailabilityCheckBackgroundService> _logger;

    public AvailabilityCheckBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AvailabilityCheckBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        do
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل تشغيل جولة التحقق الدوري من توفر الإعلانات");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var checker = scope.ServiceProvider.GetRequiredService<IListingAvailabilityChecker>();

        var settings = await unitOfWork.Settings.GetAsync();
        if (!settings.AvailabilityCheckEnabled || settings.AvailabilityCheckIntervalDays is not int days || days <= 0)
        {
            return;
        }

        var dueListings = await unitOfWork.Listings.GetDueForAvailabilityCheckAsync(days);

        foreach (var listing in dueListings)
        {
            if (ct.IsCancellationRequested) break;

            await checker.TriggerCheckAsync(
                listing, Guid.Empty,
                $"مرحباً، هذا تحقّق دوري من RentConnect - مضى أكتر من {days} أيام منذ آخر تأكيد لتوفر إعلانك \"{listing.Title}\".\nهل ما زال متاحاً أم تم تأجيره؟ أجب بـ \"نعم\" إذا تم تأجيره، أو \"لا\" إذا ما زال متاحاً.");
        }
    }
}
