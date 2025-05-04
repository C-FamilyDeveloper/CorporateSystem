using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;

namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;

public interface IDocumentUserRepository
{
    Task<DocumentUser?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentUser>> GetAsync(
        DocumentUserFilter? filter = null, 
        CancellationToken cancellationToken = default);

    Task<IEnumerable<DocumentInfo>> GetAsync(DocumentInfoFilter? filter = null, CancellationToken cancellationToken = default);
    Task<int[]> CreateAsync(CreateDocumentUserDto[] dtos, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateDocumentUserDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(DocumentUserFilter? filter = null, CancellationToken cancellationToken = default);
}