using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;

namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;

public interface IDocumentCompositeRepository
{
    Task<IEnumerable<DocumentInfo>> GetAsync(DocumentInfoFilter? filter = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentInfo>> GetAsync(int userId, CancellationToken cancellationToken = default);
}