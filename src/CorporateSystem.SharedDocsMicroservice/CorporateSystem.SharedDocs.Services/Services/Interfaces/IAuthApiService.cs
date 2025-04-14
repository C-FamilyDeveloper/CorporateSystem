namespace CorporateSystem.SharedDocs.Services.Services.Interfaces;

public interface IAuthApiService
{
    Task<string[]> GetUserEmailsByIdsAsync(int[] ids, CancellationToken cancellationToken = default);
    Task<int[]> GetUserIdsByEmailsAsync(string[] emails, CancellationToken cancellationToken = default);
}