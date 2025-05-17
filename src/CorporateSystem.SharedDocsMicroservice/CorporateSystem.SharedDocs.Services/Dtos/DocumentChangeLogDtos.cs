namespace CorporateSystem.SharedDocs.Services.Dtos;

public record struct ChangeLog(int UserId, int DocumentId, string Changes, int Line);

public record struct DocumentChangeLogDto(
    int Id,
    int DocumentId,
    string UserEmail,
    string Changes,
    DateTimeOffset ChangedAt);