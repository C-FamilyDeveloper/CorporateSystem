using CorporateSystem.SharedDocs.Domain.Enums;

namespace CorporateSystem.SharedDocs.Infrastructure.Dtos;

public record struct CreateDocumentUserDto(int DocumentId, int UserId, AccessLevel AccessLevel);

public record struct UpdateDocumentUserDto(int DocumentId, int UserId, AccessLevel AccessLevel);

public class DocumentInfo
{
    public int Id { get; init; }
    public required string Title { get; init; }
}