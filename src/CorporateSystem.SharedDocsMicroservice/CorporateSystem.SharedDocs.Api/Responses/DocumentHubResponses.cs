using System.Text.Json.Serialization;

namespace CorporateSystem.SharedDocs.Api.Responses;

public class ChangeLogResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    [JsonPropertyName("document_id")]
    public int DocumentId { get; init; }
    
    [JsonPropertyName("user_email")]
    public required string UserEmail { get; init; }
    
    [JsonPropertyName("changes")] 
    public required string Changes { get; init; }
    
    [JsonPropertyName("changed_at")]
    public DateTimeOffset ChangedAt { get; init; }
}

public class JoinDocumentGroupResponse
{
    [JsonPropertyName("content")]
    public required string Content { get; init; }

    [JsonPropertyName("change_logs")] 
    public ChangeLogResponse[] ChangeLogs { get; init; } = [];
}