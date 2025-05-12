using CorporateSystem.SharedDocs.Domain.Enums;

namespace CorporateSystem.SharedDocs.Services.Dtos;

public record struct CreateDocumentDto(int OwnerId, string Title, string Content);
public record struct AddUserToDocumentDto(int DocumentId, DocumentUserInfo[] UserInfos);
public record struct UpdateDocumentContentDto(int DocumentId, int UserId, string NewContent);
public record struct DocumentUserInfo(int UserId, AccessLevel AccessLevel);
public record struct GetDocumentUsersDto(int DocumentId, int[] UserIds);
public record struct DeleteUserFromDocumentDto(int DocumentId, int UserId);
public record struct UserInfo(string Email);