namespace RentConnect.Data.Models;

internal class ListingComment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string AuthorId { get; set; } = string.Empty;
    public string AuthorDisplayName { get; set; } = string.Empty;   // لقطة من اسم المستخدم وقت كتابة التعليق
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
}
