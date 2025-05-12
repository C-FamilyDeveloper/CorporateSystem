namespace CorporateSystem.SharedDocs.Infrastructure.Dtos;

public record struct CreateDocumentChangeLog(
    int UserId,
    int DocumentId, 
    string Changes,
    int StartLine,
    int StartColumn);