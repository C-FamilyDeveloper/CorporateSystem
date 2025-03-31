﻿using System.Text.Json.Serialization;

namespace CorporateSystem.Auth.Api.Dtos.Auth;

public class AuthRequest
{
    [JsonPropertyName("email")]
    public required string Email { get; init; }
    
    [JsonPropertyName("password")]
    public required string Password { get; init; }
}

public class RegisterRequest
{
    [JsonPropertyName("email")]
    public required string Email { get; init; }
    
    [JsonPropertyName("password")]
    public required string Password { get; init; }
    
    [JsonPropertyName("repeated_password")]
    public required string RepeatedPassword { get; init; }
}

public class SuccessRegisterRequest
{
    [JsonPropertyName("email")]
    public required string Email { get; init; }
    
    [JsonPropertyName("password")]
    public required string Password { get; init; }
    
    [JsonPropertyName("success_code")]
    public int SuccessCode { get; init; }
}

public class AuthResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }
}

public class TokenValidationRequest
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }
}

public class UserInfo
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    [JsonPropertyName("role")]
    public required string Role { get; init; }
}