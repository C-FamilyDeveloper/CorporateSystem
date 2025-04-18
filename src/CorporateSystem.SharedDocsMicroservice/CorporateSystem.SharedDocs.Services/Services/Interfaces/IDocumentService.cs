using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Services.Dtos;

namespace CorporateSystem.SharedDocs.Services.Services.Interfaces;

public interface IDocumentService
{
    Task<int> CreateDocumentAsync(CreateDocumentDto dto, CancellationToken cancellationToken = default);
    Task AddUsersToDocumentAsync(AddUserToDocumentDto dto, CancellationToken cancellationToken = default);
    Task<string[]> GetUserEmailsOfCurrentDocumentAsync(int documentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentUser>> GetDocumentUsersAsync(GetDocumentUsersDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<Document>> GetCurrentUserDocuments(int userId, CancellationToken cancellationToken = default);
    Task UpdateDocumentContentAsync(UpdateDocumentContentDto dto, CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(int[] ids, CancellationToken cancellationToken = default);
}