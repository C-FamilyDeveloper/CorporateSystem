using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Services.Dtos;
using CreateDocumentDto = CorporateSystem.SharedDocs.Services.Dtos.CreateDocumentDto;

namespace CorporateSystem.SharedDocs.Services.Services.Interfaces;

public interface IDocumentService
{
    Task<Document> GetDocumentAsync(int documentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentInfoDto>> GetDocumentsAsync(CancellationToken cancellationToken = default);
    Task<int> CreateDocumentAsync(CreateDocumentDto dto, CancellationToken cancellationToken = default);
    Task AddUsersToDocumentAsync(AddUserToDocumentDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentUser>> GetDocumentUsersAsync(GetDocumentUsersDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentInfo>> GetCurrentUserDocuments(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserInfo>> GetUsersOfCurrentDocument(int documentId, CancellationToken cancellationToken = default);
    Task<string> UpdateDocumentContentAsync(UpdateDocumentContentDto dto, CancellationToken cancellationToken = default);
    Task DeleteUsersFromCurrentDocumentAsync(DeleteUserFromDocumentDto dto, CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(DeleteDocumentDto dto, CancellationToken cancellationToken = default);
    Task DeleteDocumentUsersAsync(DeleteDocumentUsersDto dto, CancellationToken cancellationToken = default);
}