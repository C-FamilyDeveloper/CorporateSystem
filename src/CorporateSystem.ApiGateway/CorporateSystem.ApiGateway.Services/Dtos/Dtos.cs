using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Primitives;

namespace CorporateSystem.ApiGateway.Services.Dtos;

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

public class TokenValidationRequest
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }
}
