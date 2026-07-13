using Microsoft.EntityFrameworkCore;
using RentConnect.Data.Data;
using RentConnect.Data.Dtos;
using RentConnect.Data.Models;

namespace RentConnect.Data.Repositories;

internal class BillingRepository : IBillingRepository
{
    private readonly AppDbContext _db;

    public BillingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ChargeForRevealAsync(string userId, Guid listingId, decimal amountJod)
    {
        if (amountJod <= 0) return false;

        var alreadyCharged = await _db.PhoneRevealCharges
            .AnyAsync(c => c.UserId == userId && c.ListingId == listingId);
        if (alreadyCharged) return false;

        _db.PhoneRevealCharges.Add(new PhoneRevealCharge
        {
            UserId = userId,
            ListingId = listingId,
            AmountJod = amountJod,
        });

        return true;
    }

    public async Task<bool> HasChargedAsync(string userId, Guid listingId)
    {
        return await _db.PhoneRevealCharges.AnyAsync(c => c.UserId == userId && c.ListingId == listingId);
    }

    public async Task<decimal> GetUnsettledTotalAsync(string userId)
    {
        return await _db.PhoneRevealCharges
            .Where(c => c.UserId == userId && c.SettledAt == null && c.CancelledAt == null)
            .SumAsync(c => (decimal?)c.AmountJod) ?? 0;
    }

    public async Task<Dictionary<string, decimal>> GetUnsettledTotalsAsync()
    {
        return await _db.PhoneRevealCharges
            .Where(c => c.SettledAt == null && c.CancelledAt == null)
            .GroupBy(c => c.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(c => c.AmountJod) })
            .ToDictionaryAsync(x => x.UserId, x => x.Total);
    }

    public async Task<List<PhoneRevealChargeDto>> GetChargesAsync(string userId)
    {
        var charges = await _db.PhoneRevealCharges
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Join(_db.Listings, c => c.ListingId, l => l.Id, (c, l) => new { Charge = c, l.Title })
            .ToListAsync();

        return charges
            .Select(x => new PhoneRevealChargeDto(
                x.Charge.Id, x.Charge.ListingId, x.Title, x.Charge.AmountJod, x.Charge.CreatedAt,
                x.Charge.SettledAt != null, x.Charge.CancelledAt != null))
            .ToList();
    }

    public async Task<bool> SettleUserAsync(string userId)
    {
        var unsettled = await _db.PhoneRevealCharges
            .Where(c => c.UserId == userId && c.SettledAt == null && c.CancelledAt == null)
            .ToListAsync();

        if (unsettled.Count == 0) return false;

        var now = DateTime.UtcNow;
        foreach (var charge in unsettled)
        {
            charge.SettledAt = now;
        }

        return true;
    }

    public async Task<int> GetCancelledTodayCountAsync(string userId)
    {
        var todayStart = DateTime.UtcNow.Date;

        // البلاغات اللي صاحب العقار أكّد صحّتها (فعلاً مؤجّر) ما بتُحتسب من الحد الأقصى اليومي -
        // الحد مقصود للحد من البلاغات الكاذبة/غير المؤكّدة، مش البلاغات الصحيحة
        var confirmedRentedChargeIds = _db.OwnerRentedConfirmations
            .Where(oc => oc.Answer == true)
            .Select(oc => oc.ChargeId);

        return await _db.PhoneRevealCharges
            .Where(c => c.UserId == userId && c.CancelledAt != null && c.CancelledAt >= todayStart)
            .Where(c => !confirmedRentedChargeIds.Contains(c.Id))
            .CountAsync();
    }

    public async Task<(bool Success, string? Error)> CancelChargeAsync(string userId, Guid chargeId, string reason)
    {
        const int maxCancellationsPerDay = 5;

        var charge = await _db.PhoneRevealCharges.FirstOrDefaultAsync(c => c.Id == chargeId && c.UserId == userId);
        if (charge is null) return (false, "لم يتم العثور على هذا العنصر.");

        if (charge.SettledAt is not null) return (false, "تم تسديد هذا المبلغ مسبقاً، لا يمكن إلغاؤه.");
        if (charge.CancelledAt is not null) return (false, "تم إلغاء هذا العنصر مسبقاً.");

        var cancelledToday = await GetCancelledTodayCountAsync(userId);
        if (cancelledToday >= maxCancellationsPerDay)
        {
            return (false, $"وصلت للحد الأقصى ({maxCancellationsPerDay}) لعمليات الإلغاء اليوم. حاول مرة أخرى غداً.");
        }

        charge.CancelledAt = DateTime.UtcNow;
        charge.CancelReason = reason;

        return (true, null);
    }
}
