using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentConnect.Data.Models;

namespace RentConnect.Data.Data;

internal class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<ListingContractDocument> ListingContractDocuments => Set<ListingContractDocument>();
    public DbSet<ListingStatusHistory> ListingStatusHistories => Set<ListingStatusHistory>();
    public DbSet<ListingComment> ListingComments => Set<ListingComment>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<PhoneRevealCharge> PhoneRevealCharges => Set<PhoneRevealCharge>();
    public DbSet<PendingRegistration> PendingRegistrations => Set<PendingRegistration>();
    public DbSet<OwnerRentedConfirmation> OwnerRentedConfirmations => Set<OwnerRentedConfirmation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Listing>()
            .HasMany(l => l.Images)
            .WithOne(i => i.Listing)
            .HasForeignKey(i => i.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Listing>()
            .HasMany(l => l.ContractDocuments)
            .WithOne(d => d.Listing)
            .HasForeignKey(d => d.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Listing>()
            .HasMany(l => l.StatusHistory)
            .WithOne(h => h.Listing)
            .HasForeignKey(h => h.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Listing>()
            .HasMany(l => l.Comments)
            .WithOne(c => c.Listing)
            .HasForeignKey(c => c.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Listing>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(l => l.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Listing>()
            .Property(l => l.PriceJod)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Listing>()
            .Property(l => l.AreaSqm)
            .HasPrecision(10, 2);

        modelBuilder.Entity<SiteSetting>()
            .Property(s => s.PhoneRevealFeeJod)
            .HasPrecision(10, 2);

        modelBuilder.Entity<PhoneRevealCharge>()
            .Property(c => c.AmountJod)
            .HasPrecision(10, 2);

        // يمنع احتساب رسم كشف الرقم أكثر من مرة لنفس الإعلان لنفس المستخدم على مستوى قاعدة البيانات
        modelBuilder.Entity<PhoneRevealCharge>()
            .HasIndex(c => new { c.UserId, c.ListingId })
            .IsUnique();

        // يمنع تكرار رقم الهاتف بين حسابات مختلفة (يُستثنى الحسابات بدون رقم هاتف، مثل تسجيل الدخول بگوگل)
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique()
            .HasFilter("PhoneNumber IS NOT NULL");
    }
}
