using RentConnect.Data.Data;
using RentConnect.Data.Repositories;

namespace RentConnect.Data.UnitOfWork;

internal class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private IListingRepository? _listings;
    private IUserRepository? _users;
    private ISettingsRepository? _settings;
    private IBillingRepository? _billing;
    private IPendingRegistrationRepository? _pendingRegistrations;
    private IOwnerConfirmationRepository? _ownerConfirmations;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public IListingRepository Listings => _listings ??= new ListingRepository(_db);
    public IUserRepository Users => _users ??= new UserRepository(_db);
    public ISettingsRepository Settings => _settings ??= new SettingsRepository(_db);
    public IBillingRepository Billing => _billing ??= new BillingRepository(_db);
    public IPendingRegistrationRepository PendingRegistrations => _pendingRegistrations ??= new PendingRegistrationRepository(_db);
    public IOwnerConfirmationRepository OwnerConfirmations => _ownerConfirmations ??= new OwnerConfirmationRepository(_db);

    public Task<int> CompleteAsync() => _db.SaveChangesAsync();
}
