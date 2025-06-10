using CorporateSystem.SharedDocs.Domain.Enums;

namespace CorporateSystem.SharedDocs.Services.Dtos;

public record struct CreateDocumentDto(int OwnerId, string Title, string Content);
public record struct AddUserToDocumentDto(int DocumentId, DocumentUserInfo[] UserInfos);
public record struct UpdateDocumentContentDto(int DocumentId, int UserId, string NewContent, int Line);
public record struct DocumentUserInfo(int UserId, AccessLevel AccessLevel);
public record struct GetDocumentUsersDto(int DocumentId, int[] UserIds);
public record struct DeleteUserFromDocumentDto(int DocumentId, int UserId);

public record struct DocumentInfoDto(
    int Id,
    string Title,
    string OwnerEmail,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt);
    
public record struct DeleteDocumentDto(
    int[]? Ids = null,
    int[]? OwnerIds = null,
    string[]? Titles = null,
    string[]? Contents = null,
    DateTimeOffset[]? ModifiedAt = null,
    DateTimeOffset[]? CreatedAt = null);

public record struct DeleteDocumentUsersDto(
    int[]? Ids = null,
    int[]? DocumentIds = null,
    int[]? UserIds = null,
    AccessLevel[]? AccessLevels = null);

public record struct UserInfo(string Email);
