using Microsoft.EntityFrameworkCore;
using RentConnect.Data.Data;
using RentConnect.Data.Dtos;
using RentConnect.Data.Models;

namespace RentConnect.Data.Repositories;

internal class PendingRegistrationRepository : IPendingRegistrationRepository
{
    private const int MaxAttempts = 5;

    private readonly AppDbContext _db;

    public PendingRegistrationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task RemoveExistingForAsync(string email, string phoneNumber)
    {
        var existing = await _db.PendingRegistrations
            .Where(p => p.Email == email || p.PhoneNumber == phoneNumber)
            .ToListAsync();

        if (existing.Count > 0)
        {
            _db.PendingRegistrations.RemoveRange(existing);
        }
    }

    public Task<Guid> CreateAsync(string email, string phoneNumber, string? occupation, string passwordHash, string otpCode, TimeSpan validFor)
    {
        var now = DateTime.UtcNow;
        var pending = new PendingRegistration
        {
            Email = email,
            PhoneNumber = phoneNumber,
            Occupation = occupation,
            PasswordHash = passwordHash,
            OtpCode = otpCode,
            CreatedAt = now,
            LastSentAt = now,
            ExpiresAt = now.Add(validFor),
        };

        _db.PendingRegistrations.Add(pending);

        // Id يتولّد فوراً بالذاكرة (قيمة افتراضية بالموديل) - ما في داعي للحفظ الفعلي قبل إرجاعه
        return Task.FromResult(pending.Id);
    }

    public async Task<PendingRegistrationDto?> GetAsync(Guid id)
    {
        var pending = await _db.PendingRegistrations.FindAsync(id);
        return pending is null ? null : ToDto(pending);
    }

    public async Task<(OtpVerifyResult Result, PendingRegistrationDto? Verified)> VerifyAsync(Guid id, string code)
    {
        var pending = await _db.PendingRegistrations.FindAsync(id);
        if (pending is null) return (OtpVerifyResult.NotFound, null);

        if (pending.Attempts >= MaxAttempts) return (OtpVerifyResult.TooManyAttempts, null);

        if (pending.ExpiresAt < DateTime.UtcNow) return (OtpVerifyResult.Expired, null);

        if (!string.Equals(pending.OtpCode, code, StringComparison.Ordinal))
        {
            pending.Attempts++;
            return (OtpVerifyResult.InvalidCode, null);
        }

        var dto = ToDto(pending);
        return (OtpVerifyResult.Success, dto);
    }

    public async Task<PendingRegistrationDto?> RegenerateCodeAsync(Guid id, string newCode, TimeSpan validFor, TimeSpan resendCooldown)
    {
        var pending = await _db.PendingRegistrations.FindAsync(id);
        if (pending is null) return null;

        if (DateTime.UtcNow - pending.LastSentAt < resendCooldown) return null;

        var now = DateTime.UtcNow;
        pending.OtpCode = newCode;
        pending.Attempts = 0;
        pending.LastSentAt = now;
        pending.ExpiresAt = now.Add(validFor);

        return ToDto(pending);
    }

    public async Task RemoveAsync(Guid id)
    {
        var pending = await _db.PendingRegistrations.FindAsync(id);
        if (pending is not null)
        {
            _db.PendingRegistrations.Remove(pending);
        }
    }

    private static PendingRegistrationDto ToDto(PendingRegistration p) =>
        new(p.Id, p.Email, p.PhoneNumber, p.Occupation, p.PasswordHash, p.ExpiresAt, p.LastSentAt);
}
