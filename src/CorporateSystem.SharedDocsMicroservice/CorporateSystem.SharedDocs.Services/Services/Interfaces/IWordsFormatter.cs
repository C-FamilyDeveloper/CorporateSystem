namespace CorporateSystem.SharedDocs.Services.Services.Interfaces;

public interface IWordsFormatter
{
    Task<string> FormatWordAsync(string word, CancellationToken cancellationToken = default);
}