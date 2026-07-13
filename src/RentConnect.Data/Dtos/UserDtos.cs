namespace RentConnect.Data.Dtos;

public record UserDto(
    string Id,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    string? Occupation,
    bool IsAdmin,
    bool IsBanned,
    DateTime CreatedAt
);
