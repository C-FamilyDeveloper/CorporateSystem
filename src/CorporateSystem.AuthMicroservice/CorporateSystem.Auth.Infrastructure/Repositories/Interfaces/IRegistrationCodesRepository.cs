namespace CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;

public interface IRegistrationCodesRepository
{
    Task<int?> GetAsync(int code, CancellationToken cancellationToken = default);
    Task CreateAsync(int code, CancellationToken cancellationToken = default);
    Task DeleteAsync(int code, CancellationToken cancellationToken = default);
}