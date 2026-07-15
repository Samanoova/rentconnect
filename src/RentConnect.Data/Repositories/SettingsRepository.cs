using Microsoft.EntityFrameworkCore;
using RentConnect.Data.Data;
using RentConnect.Data.Dtos;
using RentConnect.Data.Models;

namespace RentConnect.Data.Repositories;

internal class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext _db;

    public SettingsRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SiteSettingDto> GetAsync()
    {
        var settings = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
        return settings is null
            ? new SiteSettingDto(0, null, null)
            : new SiteSettingDto(
                settings.PhoneRevealFeeJod,
                settings.CliqAlias,
                settings.CliqAccountName,
                settings.EvolutionApiBaseUrl,
                settings.EvolutionApiKey,
                settings.EvolutionApiInstanceName,
                settings.AvailabilityCheckEnabled,
                settings.AvailabilityCheckIntervalDays,
                settings.MaxListingsPerUser);
    }

    public async Task UpdateAsync(SiteSettingUpdateDto dto)
    {
        var settings = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Id == 1);
        if (settings is null)
        {
            settings = new SiteSetting { Id = 1 };
            _db.SiteSettings.Add(settings);
        }

        settings.PhoneRevealFeeJod = dto.PhoneRevealFeeJod;
        settings.CliqAlias = dto.CliqAlias;
        settings.CliqAccountName = dto.CliqAccountName;
        settings.EvolutionApiBaseUrl = dto.EvolutionApiBaseUrl;
        settings.EvolutionApiKey = dto.EvolutionApiKey;
        settings.EvolutionApiInstanceName = dto.EvolutionApiInstanceName;
        settings.AvailabilityCheckEnabled = dto.AvailabilityCheckEnabled;
        settings.AvailabilityCheckIntervalDays = dto.AvailabilityCheckIntervalDays;
        settings.MaxListingsPerUser = dto.MaxListingsPerUser;
    }
}
