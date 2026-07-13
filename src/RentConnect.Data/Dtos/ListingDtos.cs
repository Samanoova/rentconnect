using RentConnect.Data.Models;

namespace RentConnect.Data.Dtos;

public record ListingImageDto(Guid Id, string Url, int SortOrder);

public record ListingContractDocumentDto(Guid Id, string Url, string FileName, int SortOrder);

// ملف عقد جديد قبل ما يُحفظ - يُستخدم فقط عند الإضافة (السيرفر يحدّد Id وSortOrder بنفسه)
public record ContractDocumentUpload(string Url, string FileName);

public record ListingCommentDto(Guid Id, string AuthorId, string AuthorDisplayName, string Content, DateTime CreatedAt);

public record ListingDto(
    Guid Id,
    string Title,
    string? Description,
    PropertyType PropertyType,
    decimal PriceJod,
    string Region,
    string? PreciseAddress,
    double? Latitude,
    double? Longitude,
    int Bedrooms,
    int Bathrooms,
    decimal? AreaSqm,
    int? Floor,
    bool IsFurnished,
    bool HasYard,
    bool HasBalcony,
    bool HasElevator,
    bool HasGarage,
    WaterMeterType WaterMeterType,
    int? WaterCubicMeters,
    ElectricityMeterType ElectricityMeterType,
    bool IsElectricitySubsidized,
    string OwnerPhone,
    TenantPreference TenantPreference,
    bool RequiresEmployedTenant,
    int PaymentIntervalMonths,
    SecurityGuaranteeType SecurityGuarantee,
    decimal? SecurityDepositJod,
    bool HasRentalContract,
    ListingStatus Status,
    string? OwnerId,
    bool IsDisabledByAdmin,
    int ViewCount,
    int PhoneRevealCount,
    DateTime CreatedAt,
    DateTime LastConfirmedAt,
    List<ListingImageDto> Images,
    List<ListingContractDocumentDto> ContractDocuments,
    string? SourceUrl = null
);

public record ListingCreateDto(
    string Title,
    string? Description,
    PropertyType PropertyType,
    decimal PriceJod,
    string Region,
    string? PreciseAddress,
    double? Latitude,
    double? Longitude,
    int Bedrooms,
    int Bathrooms,
    decimal? AreaSqm,
    int? Floor,
    bool IsFurnished,
    bool HasYard,
    bool HasBalcony,
    bool HasElevator,
    bool HasGarage,
    WaterMeterType WaterMeterType,
    int? WaterCubicMeters,
    ElectricityMeterType ElectricityMeterType,
    bool IsElectricitySubsidized,
    string OwnerPhone,
    TenantPreference TenantPreference,
    bool RequiresEmployedTenant,
    int PaymentIntervalMonths,
    SecurityGuaranteeType SecurityGuarantee,
    decimal? SecurityDepositJod,
    bool HasRentalContract,
    string? OwnerId,
    List<string>? ImageUrls,
    List<ContractDocumentUpload>? ContractDocuments,
    string? SourceUrl = null
);

public record ListingUpdateDto(
    string Title,
    string? Description,
    PropertyType PropertyType,
    decimal PriceJod,
    string Region,
    string? PreciseAddress,
    double? Latitude,
    double? Longitude,
    int Bedrooms,
    int Bathrooms,
    decimal? AreaSqm,
    int? Floor,
    bool IsFurnished,
    bool HasYard,
    bool HasBalcony,
    bool HasElevator,
    bool HasGarage,
    WaterMeterType WaterMeterType,
    int? WaterCubicMeters,
    ElectricityMeterType ElectricityMeterType,
    bool IsElectricitySubsidized,
    string OwnerPhone,
    TenantPreference TenantPreference,
    bool RequiresEmployedTenant,
    int PaymentIntervalMonths,
    SecurityGuaranteeType SecurityGuarantee,
    decimal? SecurityDepositJod,
    bool HasRentalContract,
    string? SourceUrl = null
);

public record ListingStatusUpdateDto(ListingStatus NewStatus);
