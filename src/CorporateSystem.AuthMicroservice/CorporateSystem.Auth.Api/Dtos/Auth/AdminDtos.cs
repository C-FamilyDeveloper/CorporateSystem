using System.Text.Json.Serialization;
using CorporateSystem.Auth.Domain.Enums;

namespace CorporateSystem.Auth.Api.Dtos.Auth;

public class DeleteUsersRequest
{
    [JsonPropertyName("user_ids")] 
    public int[] UserIds { get; init; } = [];
}

public class GetUsersRequest
{
    [JsonPropertyName("page")]
    public int Page { get; init; }
    
    [JsonPropertyName("per_page")]
    public int PerPage { get; init; }
}

public class GetUsersResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    [JsonPropertyName("email")]
    public required string Email { get; init; }
    
    [JsonPropertyName("first_name")]
    public required string FirstName { get; init; }
    
    [JsonPropertyName("last_name")]
    public required string LastName { get; init; }
    
    [JsonPropertyName("gender")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Gender Gender { get; init; }
    
    [JsonPropertyName("role")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Role Role { get; init; }
}