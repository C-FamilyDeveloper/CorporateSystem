using System.Text.Json.Serialization;
using CorporateSystem.SharedDocs.Domain.Enums;

namespace CorporateSystem.SharedDocs.Api.Requests;

public class AddUserToDocumentRequest
{
    [JsonPropertyName("document_user_infos")]
    public required DocumentUserInfoDto[] DocumentUserInfos { get; init; }
    
    [JsonPropertyName("document_id")]
    public required int DocumentId { get; init; }
}

public class DocumentUserInfoDto
{
    [JsonPropertyName("user_email")]
    public required string UserEmail { get; init; }
    
    [JsonPropertyName("access_level")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AccessLevel AccessLevel { get; init; }
}

public class CreateDocumentRequest
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }
}