namespace RentConnect.Data.Dtos;

public record SiteSettingDto(
    decimal PhoneRevealFeeJod,
    string? CliqAlias,
    string? CliqAccountName,
    string? EvolutionApiBaseUrl = null,
    string? EvolutionApiKey = null,
    string? EvolutionApiInstanceName = null,
    bool AvailabilityCheckEnabled = false,
    int? AvailabilityCheckIntervalDays = null);

public record SiteSettingUpdateDto(
    decimal PhoneRevealFeeJod,
    string? CliqAlias,
    string? CliqAccountName,
    string? EvolutionApiBaseUrl = null,
    string? EvolutionApiKey = null,
    string? EvolutionApiInstanceName = null,
    bool AvailabilityCheckEnabled = false,
    int? AvailabilityCheckIntervalDays = null);

public record PhoneRevealChargeDto(Guid Id, Guid ListingId, string ListingTitle, decimal AmountJod, DateTime CreatedAt, bool IsSettled, bool IsCancelled);
