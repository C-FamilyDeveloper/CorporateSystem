using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CorporateSystem.Auth.Api.Dtos.Auth;
using CorporateSystem.Auth.Domain.Enums;
using CorporateSystem.Auth.Infrastructure;
using CorporateSystem.Auth.Services.Exceptions;
using CorporateSystem.Auth.Services.Options;
using CorporateSystem.Auth.Services.Services.Interfaces;
using CorporateSystem.Auth.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit.Abstractions;

namespace CorporateSystem.Auth.Tests.IntegrationTests;

public class AuthControllerTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    private readonly string TestSecretKey = factory.TestSecretKey;

    [Fact]
    public async Task ValidateToken_ValidTokenObtainedFromSignIn_ReturnsUserInfo()
    {
        // Arrange
        var authRequest = new AuthRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var token = JwtHelper.GenerateJwtToken(TestSecretKey, 1, authRequest.Email);

        factory.MockAuthService.Setup(service => service.AuthenticateAsync(
                It.IsAny<AuthUserDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResultDto(token));

        factory.MockTokenService
            .Setup(x => x.ValidateTokenAsync(It.Is<string>(str => str == token), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var signInResponse = await client.PostAsJsonAsync("/api/auth/sign-in", authRequest);
        signInResponse.EnsureSuccessStatusCode();

        var authResponse = await signInResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.NotNull(authResponse.Token);
        Assert.Equal(token, authResponse.Token);
        
        var validateTokenRequest = new TokenValidationRequest
        {
            Token = authResponse.Token
        };

        // Act
        var validateTokenResponse = await client.PostAsJsonAsync("/api/auth/validate-token", validateTokenRequest);
        
        // Assert
        validateTokenResponse.EnsureSuccessStatusCode();
        var userInfo = await validateTokenResponse.Content.ReadFromJsonAsync<UserInfo>();

        Assert.NotNull(userInfo);
        
        factory.MockTokenService.Verify(service => 
            service.ValidateTokenAsync(
                validateTokenRequest.Token,
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task ValidateToken_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var token = "invalid-token";

        factory.MockTokenService.Setup(service => service.ValidateTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new TokenValidationRequest { Token = token };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/validate-token", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        factory.MockTokenService.Verify(service => 
            service.ValidateTokenAsync(
                token, 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateToken_ExceptionDuringValidation_ReturnsUnauthorized()
    {
        // Arrange
        var token = JwtHelper.GenerateJwtToken(TestSecretKey, 1, "someEmail@test.com");

        factory.MockTokenService.Setup(service => 
                service.ValidateTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new TokenValidationRequest { Token = token };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/validate-token", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        factory.MockTokenService.Verify(service => 
            service.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public void Dispose()
    {
        factory.ResetMocks();
    }
}