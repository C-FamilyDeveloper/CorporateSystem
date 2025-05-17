namespace CorporateSystem.SharedDocs.Infrastructure.Dtos;

public record struct CreateDocumentChangeLogDto(
    int UserId,
    int DocumentId, 
    string Changes,
    int Line);
    
public record struct UpdateDocumentChangeLogDto(string Changes);