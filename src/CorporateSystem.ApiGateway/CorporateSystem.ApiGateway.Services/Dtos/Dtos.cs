using System.Text.Json.Serialization;

namespace CorporateSystem.ApiGateway.Services.Dtos;

public class UserInfo
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    [JsonPropertyName("role")]
    public required string Role { get; init; }
}

public class TokenValidationRequest
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }
}
