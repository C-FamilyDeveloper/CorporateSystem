namespace CorporateSystem.SharedDocs.Services.Dtos;

public record struct ChangeLog(int UserId, int DocumentId, string Changes, int StartLine, int StartColumn);