using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;

namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;

public interface IDocumentChangeLogRepository
{
    Task<IEnumerable<DocumentChangeLog>> GetAsync(
        DocumentChangeLogFilter? filter = null,
        CancellationToken cancellationToken = default);

    Task<int[]> CreateAsync(CreateDocumentChangeLog[] dtos, CancellationToken cancellationToken = default);
    Task DeleteAsync(DocumentChangeLogFilter? filter = null, CancellationToken cancellationToken = default);
}