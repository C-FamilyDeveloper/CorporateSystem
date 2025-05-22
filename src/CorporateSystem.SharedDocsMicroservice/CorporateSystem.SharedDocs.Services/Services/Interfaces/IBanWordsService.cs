namespace CorporateSystem.SharedDocs.Services.Services.Interfaces;

public interface IBanWordsService
{
    Task<string> ProcessTextAsync(string content, CancellationToken cancellationToken = default);
}