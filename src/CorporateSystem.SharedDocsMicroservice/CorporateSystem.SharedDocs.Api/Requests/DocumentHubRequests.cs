using System.Text.Json.Serialization;
using CorporateSystem.SharedDocs.Domain.Enums;

namespace CorporateSystem.SharedDocs.Api.Requests;

public class JoinDocumentGroupRequest
{
    [JsonPropertyName("document_id")]
    public required int DocumentId { get; init; }
    
    [JsonPropertyName("user_id")]
    public required int UserId { get; init; }
    
    [JsonPropertyName("access_level")]
    public required AccessLevel AccessLevel { get; init; }
}

public class SendDocumentUpdateRequest
{
    [JsonPropertyName("document_id")]
    public required int DocumentId { get; init; }
    
    [JsonPropertyName("user_id")]
    public required int UserId { get; init; }
    
    [JsonPropertyName("new_content")]
    public required string NewContent { get; init; }
}

public class UserInfo
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("role")]
    public required string Role { get; init; }
}