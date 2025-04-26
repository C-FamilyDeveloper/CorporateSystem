namespace CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;

public interface IRegistrationCodesRepository
{
    Task<int?> GetAsync(object[] identifiers, CancellationToken cancellationToken = default);
    Task CreateAsync(object[] identifiers, int code, CancellationToken cancellationToken = default);
    Task DeleteAsync(object[] identifiers, CancellationToken cancellationToken = default);
}