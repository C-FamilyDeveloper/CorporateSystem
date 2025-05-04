using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;

namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Document>> GetAsync(DocumentFilter? filter = null, CancellationToken cancellationToken = default);
    Task<int[]> CreateAsync(CreateDocumentDto[] dtos, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateDocumentDto updatedDocumentDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(DocumentFilter? filter = null, CancellationToken cancellationToken = default);
}