namespace CorporateSystem.SharedDocs.Infrastructure.Dtos;

public record struct CreateDocumentDto(int OwnerId, string Title, string Content);
public record struct UpdateDocumentDto(string Title, string Content);