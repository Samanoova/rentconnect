namespace RentConnect.Data.Models;

internal class ListingContractDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
}
