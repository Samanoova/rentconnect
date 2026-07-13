using RentConnect.Data.Dtos;

namespace RentConnect.Data.Repositories;

public interface ISettingsRepository
{
    Task<SiteSettingDto> GetAsync();
    Task UpdateAsync(SiteSettingUpdateDto dto);
}
