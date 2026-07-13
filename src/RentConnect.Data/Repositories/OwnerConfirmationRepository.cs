using Microsoft.EntityFrameworkCore;
using RentConnect.Data.Data;
using RentConnect.Data.Models;

namespace RentConnect.Data.Repositories;

internal class OwnerConfirmationRepository : IOwnerConfirmationRepository
{
    private readonly AppDbContext _db;

    public OwnerConfirmationRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task CreateAsync(Guid listingId, Guid chargeId, string ownerPhoneWhatsApp)
    {
        _db.OwnerRentedConfirmations.Add(new OwnerRentedConfirmation
        {
            ListingId = listingId,
            ChargeId = chargeId,
            OwnerPhone = ownerPhoneWhatsApp,
        });

        return Task.CompletedTask;
    }

    public async Task<Guid?> AnswerLatestPendingAsync(string ownerPhoneWhatsApp, bool answer)
    {
        var pending = await _db.OwnerRentedConfirmations
            .Where(c => c.OwnerPhone == ownerPhoneWhatsApp && c.AnsweredAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (pending is null) return null;

        pending.AnsweredAt = DateTime.UtcNow;
        pending.Answer = answer;

        return pending.ListingId;
    }
}
