using RentConnect.Data.Repositories;

namespace RentConnect.Data.UnitOfWork;

public interface IUnitOfWork
{
    IListingRepository Listings { get; }
    IUserRepository Users { get; }
    ISettingsRepository Settings { get; }
    IBillingRepository Billing { get; }
    IPendingRegistrationRepository PendingRegistrations { get; }
    IOwnerConfirmationRepository OwnerConfirmations { get; }

    /// <summary>
    /// يحفظ كل التغييرات المُجهّزة عبر المستودعات (Add/Update/Delete) دفعة واحدة.
    /// </summary>
    Task<int> CompleteAsync();
}
