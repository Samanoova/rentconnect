namespace RentConnect.Data.Dtos;

public record PendingRegistrationDto(
    Guid Id,
    string Email,
    string PhoneNumber,
    string? Occupation,
    string PasswordHash,
    DateTime ExpiresAt,
    DateTime LastSentAt);

public enum OtpVerifyResult
{
    Success,
    InvalidCode,
    Expired,
    NotFound,
    TooManyAttempts,
}
