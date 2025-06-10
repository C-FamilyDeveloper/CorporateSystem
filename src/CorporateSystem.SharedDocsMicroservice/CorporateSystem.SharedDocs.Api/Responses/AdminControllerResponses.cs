using System.Text.Json.Serialization;

namespace CorporateSystem.SharedDocs.Api.Responses;

public class GetDocumentsForAdminResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    [JsonPropertyName("title")]
    public required string Title { get; init; }
    
    [JsonPropertyName("owner_email")]
    public required string OwnerEmail { get; init; }
    
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
    
    [JsonPropertyName("modified_at")]
    public DateTimeOffset? ModifiedAt { get; init; }
}

public class GetDocumentContentResponse
{
    [JsonPropertyName("content")]
    public required string Content { get; init; }
}