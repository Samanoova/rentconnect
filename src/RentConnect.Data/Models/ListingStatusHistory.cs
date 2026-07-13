namespace RentConnect.Data.Models;

internal class ListingStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }

    public ListingStatus OldStatus { get; set; }
    public ListingStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
