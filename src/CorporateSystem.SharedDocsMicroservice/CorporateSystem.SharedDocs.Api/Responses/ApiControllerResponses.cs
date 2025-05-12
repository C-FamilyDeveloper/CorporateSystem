using System.Text.Json.Serialization;

namespace CorporateSystem.SharedDocs.Api.Responses;

public class GetDocumentsResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    [JsonPropertyName("title")]
    public required string Title { get; init; }
    
    [JsonPropertyName("is_owner")]
    public bool IsOwner { get; init; }
}

public class CreateDocumentResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

public class GetUsersOfCurrentDocument
{
    [JsonPropertyName("email")]
    public required string Email { get; set; }
}