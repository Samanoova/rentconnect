using Microsoft.EntityFrameworkCore;
using RentConnect.Data.Data;
using RentConnect.Data.Dtos;
using RentConnect.Data.Identity;

namespace RentConnect.Data.Repositories;

internal class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var adminRoleId = await _db.Roles
            .Where(r => r.Name == RoleNames.Admin)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var adminUserIds = adminRoleId is null
            ? new HashSet<string>()
            : (await _db.UserRoles
                .Where(ur => ur.RoleId == adminRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync())
                .ToHashSet();

        var users = await _db.Users.OrderBy(u => u.CreatedAt).ToListAsync();

        return users
            .Select(u => new UserDto(u.Id, u.UserName, u.Email, u.PhoneNumber, u.Occupation, adminUserIds.Contains(u.Id), u.IsBanned, u.CreatedAt))
            .ToList();
    }

    public async Task<bool> SetBannedAsync(string userId, bool isBanned)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return false;

        user.IsBanned = isBanned;
        return true;
    }
}
