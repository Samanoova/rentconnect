using Microsoft.EntityFrameworkCore;
using RentConnect.Data.Data;
using RentConnect.Data.Dtos;
using RentConnect.Data.Models;
using RentConnect.Data.Utilities;

namespace RentConnect.Data.Repositories;

internal class ListingRepository : IListingRepository
{
    private readonly AppDbContext _db;

    public ListingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ListingDto>> GetAllAsync(
        string? region = null,
        ListingStatus? status = null,
        string? ownerId = null,
        bool? disabledByAdmin = null,
        PropertyType? propertyType = null,
        bool? hasYard = null,
        bool? hasBalcony = null,
        bool? hasElevator = null,
        bool? hasGarage = null)
    {
        var query = _db.Listings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(region))
            query = query.Where(l => l.Region.Contains(region));

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(ownerId))
            query = query.Where(l => l.OwnerId == ownerId);

        if (disabledByAdmin.HasValue)
            query = query.Where(l => l.IsDisabledByAdmin == disabledByAdmin.Value);

        if (propertyType.HasValue)
            query = query.Where(l => l.PropertyType == propertyType.Value);

        if (hasYard == true)
            query = query.Where(l => l.HasYard);

        if (hasBalcony == true)
            query = query.Where(l => l.HasBalcony);

        if (hasElevator == true)
            query = query.Where(l => l.HasElevator);

        if (hasGarage == true)
            query = query.Where(l => l.HasGarage);

        var listings = await query
            .Include(l => l.Images)
            .Include(l => l.ContractDocuments)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return listings.Select(ToDto).ToList();
    }

    public async Task<ListingDto?> GetByIdAsync(Guid id)
    {
        var listing = await _db.Listings
            .Include(l => l.Images)
            .Include(l => l.ContractDocuments)
            .FirstOrDefaultAsync(l => l.Id == id);

        return listing is null ? null : ToDto(listing);
    }

    public Task<ListingDto> AddAsync(ListingCreateDto dto)
    {
        var listing = new Listing
        {
            Title = TextSanitizer.StripPhoneNumbers(dto.Title) ?? string.Empty,
            Description = TextSanitizer.StripPhoneNumbers(dto.Description),
            PropertyType = dto.PropertyType,
            PriceJod = dto.PriceJod,
            Region = TextSanitizer.StripPhoneNumbers(dto.Region) ?? string.Empty,
            PreciseAddress = TextSanitizer.StripPhoneNumbers(dto.PreciseAddress),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            AreaSqm = dto.AreaSqm,
            Floor = dto.Floor,
            IsFurnished = dto.IsFurnished,
            HasYard = dto.HasYard,
            HasBalcony = dto.HasBalcony,
            HasElevator = dto.HasElevator,
            HasGarage = dto.HasGarage,
            WaterMeterType = dto.WaterMeterType,
            WaterCubicMeters = dto.WaterCubicMeters,
            ElectricityMeterType = dto.ElectricityMeterType,
            IsElectricitySubsidized = dto.IsElectricitySubsidized,
            OwnerPhone = dto.OwnerPhone,
            TenantPreference = dto.TenantPreference,
            RequiresEmployedTenant = dto.RequiresEmployedTenant,
            PaymentIntervalMonths = dto.PaymentIntervalMonths,
            SecurityGuarantee = dto.SecurityGuarantee,
            SecurityDepositJod = dto.SecurityDepositJod,
            HasRentalContract = dto.HasRentalContract,
            OwnerId = dto.OwnerId,
            SourceUrl = dto.SourceUrl,
        };

        if (dto.ImageUrls is not null)
        {
            for (int i = 0; i < dto.ImageUrls.Count; i++)
                listing.Images.Add(new ListingImage { Url = dto.ImageUrls[i], SortOrder = i });
        }

        if (dto.ContractDocuments is not null)
        {
            for (int i = 0; i < dto.ContractDocuments.Count; i++)
            {
                var doc = dto.ContractDocuments[i];
                listing.ContractDocuments.Add(new ListingContractDocument { Url = doc.Url, FileName = doc.FileName, SortOrder = i });
            }
        }

        // فقط تسجيل بذاكرة التعقب - الحفظ الفعلي يصير بـ UnitOfWork.CompleteAsync()
        _db.Listings.Add(listing);

        return Task.FromResult(ToDto(listing));
    }

    public async Task<bool> UpdateAsync(Guid id, ListingUpdateDto dto)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing is null) return false;

        listing.Title = TextSanitizer.StripPhoneNumbers(dto.Title) ?? string.Empty;
        listing.Description = TextSanitizer.StripPhoneNumbers(dto.Description);
        listing.PropertyType = dto.PropertyType;
        listing.PriceJod = dto.PriceJod;
        listing.Region = TextSanitizer.StripPhoneNumbers(dto.Region) ?? string.Empty;
        listing.PreciseAddress = TextSanitizer.StripPhoneNumbers(dto.PreciseAddress);
        listing.Latitude = dto.Latitude;
        listing.Longitude = dto.Longitude;
        listing.Bedrooms = dto.Bedrooms;
        listing.Bathrooms = dto.Bathrooms;
        listing.AreaSqm = dto.AreaSqm;
        listing.Floor = dto.Floor;
        listing.IsFurnished = dto.IsFurnished;
        listing.HasYard = dto.HasYard;
        listing.HasBalcony = dto.HasBalcony;
        listing.HasElevator = dto.HasElevator;
        listing.HasGarage = dto.HasGarage;
        listing.WaterMeterType = dto.WaterMeterType;
        listing.WaterCubicMeters = dto.WaterCubicMeters;
        listing.ElectricityMeterType = dto.ElectricityMeterType;
        listing.IsElectricitySubsidized = dto.IsElectricitySubsidized;
        listing.OwnerPhone = dto.OwnerPhone;
        listing.TenantPreference = dto.TenantPreference;
        listing.RequiresEmployedTenant = dto.RequiresEmployedTenant;
        listing.PaymentIntervalMonths = dto.PaymentIntervalMonths;
        listing.SecurityGuarantee = dto.SecurityGuarantee;
        listing.SecurityDepositJod = dto.SecurityDepositJod;
        listing.HasRentalContract = dto.HasRentalContract;
        listing.SourceUrl = dto.SourceUrl;

        return true;
    }

    public async Task<ListingDto?> UpdateStatusAsync(Guid id, ListingStatus newStatus)
    {
        var listing = await _db.Listings.Include(l => l.Images).Include(l => l.ContractDocuments).FirstOrDefaultAsync(l => l.Id == id);
        if (listing is null) return null;

        var oldStatus = listing.Status;
        listing.Status = newStatus;
        listing.LastConfirmedAt = DateTime.UtcNow;

        _db.ListingStatusHistories.Add(new ListingStatusHistory
        {
            ListingId = listing.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
        });

        return ToDto(listing);
    }

    public async Task<bool> SetDisabledByAdminAsync(Guid id, bool disabled)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing is null) return false;

        listing.IsDisabledByAdmin = disabled;
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing is null) return false;

        _db.Listings.Remove(listing);
        return true;
    }

    public async Task<bool> AddImagesAsync(Guid listingId, List<string> imageUrls)
    {
        var listing = await _db.Listings.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == listingId);
        if (listing is null) return false;

        var nextSortOrder = listing.Images.Count == 0 ? 0 : listing.Images.Max(i => i.SortOrder) + 1;
        foreach (var url in imageUrls)
        {
            listing.Images.Add(new ListingImage { Url = url, SortOrder = nextSortOrder++ });
        }

        return true;
    }

    public async Task<bool> RemoveImageAsync(Guid listingId, Guid imageId)
    {
        var image = await _db.ListingImages.FirstOrDefaultAsync(i => i.Id == imageId && i.ListingId == listingId);
        if (image is null) return false;

        _db.ListingImages.Remove(image);
        return true;
    }

    public async Task<bool> AddContractDocumentsAsync(Guid listingId, List<ContractDocumentUpload> documents)
    {
        var listing = await _db.Listings.Include(l => l.ContractDocuments).FirstOrDefaultAsync(l => l.Id == listingId);
        if (listing is null) return false;

        var nextSortOrder = listing.ContractDocuments.Count == 0 ? 0 : listing.ContractDocuments.Max(d => d.SortOrder) + 1;
        foreach (var doc in documents)
        {
            listing.ContractDocuments.Add(new ListingContractDocument { Url = doc.Url, FileName = doc.FileName, SortOrder = nextSortOrder++ });
        }

        return true;
    }

    public async Task<bool> RemoveContractDocumentAsync(Guid listingId, Guid documentId)
    {
        var document = await _db.ListingContractDocuments.FirstOrDefaultAsync(d => d.Id == documentId && d.ListingId == listingId);
        if (document is null) return false;

        _db.ListingContractDocuments.Remove(document);
        return true;
    }

    public async Task<bool> IncrementViewCountAsync(Guid id)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing is null) return false;

        listing.ViewCount++;
        return true;
    }

    public async Task<bool> IncrementPhoneRevealCountAsync(Guid id)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing is null) return false;

        listing.PhoneRevealCount++;
        return true;
    }

    public async Task<List<ListingCommentDto>> GetCommentsAsync(Guid listingId)
    {
        var comments = await _db.ListingComments
            .Where(c => c.ListingId == listingId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments
            .Select(c => new ListingCommentDto(c.Id, c.AuthorId, c.AuthorDisplayName, c.Content, c.CreatedAt))
            .ToList();
    }

    public async Task<bool> AddCommentAsync(Guid listingId, string authorId, string authorDisplayName, string content)
    {
        var listingExists = await _db.Listings.AnyAsync(l => l.Id == listingId);
        if (!listingExists) return false;

        _db.ListingComments.Add(new ListingComment
        {
            ListingId = listingId,
            AuthorId = authorId,
            AuthorDisplayName = authorDisplayName,
            Content = TextSanitizer.StripPhoneNumbers(content) ?? string.Empty,
        });

        return true;
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId)
    {
        var comment = await _db.ListingComments.FindAsync(commentId);
        if (comment is null) return false;

        _db.ListingComments.Remove(comment);
        return true;
    }

    public async Task<List<ListingDto>> GetStaleAsync(int days = 7, string? ownerId = null)
    {
        var threshold = DateTime.UtcNow.AddDays(-days);

        var query = _db.Listings
            .Where(l => l.Status == ListingStatus.Available && l.LastConfirmedAt < threshold);

        if (!string.IsNullOrWhiteSpace(ownerId))
            query = query.Where(l => l.OwnerId == ownerId);

        var listings = await query
            .Include(l => l.Images)
            .Include(l => l.ContractDocuments)
            .OrderBy(l => l.LastConfirmedAt)
            .ToListAsync();

        return listings.Select(ToDto).ToList();
    }

    public async Task<List<ListingDto>> GetDueForAvailabilityCheckAsync(int intervalDays)
    {
        var threshold = DateTime.UtcNow.AddDays(-intervalDays);

        var pendingListingIds = _db.OwnerRentedConfirmations
            .Where(c => c.AnsweredAt == null)
            .Select(c => c.ListingId);

        var listings = await _db.Listings
            .Where(l => l.Status == ListingStatus.Available && !l.IsDisabledByAdmin && l.LastConfirmedAt < threshold)
            .Where(l => !pendingListingIds.Contains(l.Id))
            .Include(l => l.Images)
            .Include(l => l.ContractDocuments)
            .OrderBy(l => l.LastConfirmedAt)
            .ToListAsync();

        return listings.Select(ToDto).ToList();
    }

    public async Task<bool> ConfirmStillAvailableAsync(Guid id)
    {
        var listing = await _db.Listings.FindAsync(id);
        if (listing is null) return false;

        listing.LastConfirmedAt = DateTime.UtcNow;
        return true;
    }

    private static ListingDto ToDto(Listing l) => new(
        l.Id,
        l.Title,
        l.Description,
        l.PropertyType,
        l.PriceJod,
        l.Region,
        l.PreciseAddress,
        l.Latitude,
        l.Longitude,
        l.Bedrooms,
        l.Bathrooms,
        l.AreaSqm,
        l.Floor,
        l.IsFurnished,
        l.HasYard,
        l.HasBalcony,
        l.HasElevator,
        l.HasGarage,
        l.WaterMeterType,
        l.WaterCubicMeters,
        l.ElectricityMeterType,
        l.IsElectricitySubsidized,
        l.OwnerPhone,
        l.TenantPreference,
        l.RequiresEmployedTenant,
        l.PaymentIntervalMonths,
        l.SecurityGuarantee,
        l.SecurityDepositJod,
        l.HasRentalContract,
        l.Status,
        l.OwnerId,
        l.IsDisabledByAdmin,
        l.ViewCount,
        l.PhoneRevealCount,
        l.CreatedAt,
        l.LastConfirmedAt,
        l.Images.OrderBy(i => i.SortOrder).Select(i => new ListingImageDto(i.Id, i.Url, i.SortOrder)).ToList(),
        l.ContractDocuments.OrderBy(d => d.SortOrder).Select(d => new ListingContractDocumentDto(d.Id, d.Url, d.FileName, d.SortOrder)).ToList(),
        l.SourceUrl
    );
}
