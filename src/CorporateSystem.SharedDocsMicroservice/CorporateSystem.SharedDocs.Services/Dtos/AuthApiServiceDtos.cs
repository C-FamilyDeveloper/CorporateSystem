using System.Text.Json.Serialization;

namespace CorporateSystem.SharedDocs.Services.Dtos;

public class GetUserEmailsByIdsRequest
{
    [JsonPropertyName("user_ids")]
    public required int[] UserIds { get; init; }
}

public class GetUserEmailsByIdsResponse
{
    [JsonPropertyName("user_emails")]
    public required string[] UserEmails { get; init; }
}

public class GetUserIdsByEmailsRequest
{
    [JsonPropertyName("user_emails")]
    public required string[] UserEmails { get; init; }
}

public class GetUserIdsByEmailsResponse
{
    [JsonPropertyName("user_ids")]
    public required int[] UserIds { get; init; }
}