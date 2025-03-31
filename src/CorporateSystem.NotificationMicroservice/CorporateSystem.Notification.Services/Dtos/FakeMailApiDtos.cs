using System.Text.Json.Serialization;

namespace CorporateSystem.Services.Dtos;

public class SendEmailMessageRequest
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }
    
    [JsonPropertyName("title")]
    public required string Title { get; init; }
    
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("receiverEmails")] 
    public string[] ReceiverEmails { get; init; } = [];
}

public class GetEmailByTokenRequest
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }
}

public class GetEmailByTokenResponse
{
    [JsonPropertyName("email")]
    public required string Email { get; init; }
}