using System.Text.RegularExpressions;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;

namespace CorporateSystem.SharedDocs.Services.Services.Implementations;

internal class HtmlWordFormatter : IWordsFormatter
{
    public Task<string> FormatWordAsync(string word, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var regex = new Regex(@"<u\b[^>]*>|</u>", RegexOptions.IgnoreCase);
        
        var cleanedWord = regex.Replace(word, string.Empty);
        
        var result = $"<u style=\"color: rgb(230, 0, 0);\">{cleanedWord}</u>";

        return Task.FromResult(result);
    }
}