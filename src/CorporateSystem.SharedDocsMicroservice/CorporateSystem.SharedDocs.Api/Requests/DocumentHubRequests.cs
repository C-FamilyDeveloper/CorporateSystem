using System.Text;
using System.Text.Json.Serialization;
using CorporateSystem.SharedDocs.Domain.Enums;

namespace CorporateSystem.SharedDocs.Api.Requests;

public class JoinDocumentGroupRequest
{
    [JsonPropertyName("document_id")]
    public required int DocumentId { get; init; }
}

public class SendDocumentUpdateRequest
{
    [JsonPropertyName("document_id")]
    public required int DocumentId { get; init; }
    
    [JsonPropertyName("new_content")]
    public required string NewContent { get; init; }
}

public class UserInfo
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("role")]
    public required string Role { get; init; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append("{ ");
        sb.Append($"id={Id}, role={Role}");
        sb.Append(" }");
        
        return sb.ToString();
    }
}