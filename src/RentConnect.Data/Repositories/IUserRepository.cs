using RentConnect.Data.Dtos;

namespace RentConnect.Data.Repositories;

public interface IUserRepository
{
    Task<List<UserDto>> GetAllAsync();
    Task<bool> SetBannedAsync(string userId, bool isBanned);
}
