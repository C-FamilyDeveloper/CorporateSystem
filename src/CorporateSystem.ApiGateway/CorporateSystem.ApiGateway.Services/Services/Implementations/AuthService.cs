using System.Net.Http.Json;
using CorporateSystem.ApiGateway.Services.Dtos;
using CorporateSystem.ApiGateway.Services.Options;
using CorporateSystem.ApiGateway.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CorporateSystem.ApiGateway.Services.Services.Implementations;

internal class AuthService(
    IOptions<AuthMicroserviceOptions> authMicroserviceOptions,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<UserInfo> GetUserInfoAsyncByToken(string token, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(authMicroserviceOptions.Value.Address);

        var request = new HttpRequestMessage
        {
            Content = JsonContent.Create(new TokenValidationRequest
            {
                Token = token
            }),
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{client.BaseAddress}api/auth/validate-token")
        };
        
        var response = await client.SendAsync(request, cancellationToken);
        logger.LogInformation(
            $"{nameof(GetUserInfoAsyncByToken)}: response content={await response.Content.ReadAsStringAsync(cancellationToken)}");
        
        response.EnsureSuccessStatusCode();

        var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken);
        
        ArgumentNullException.ThrowIfNull(userInfo);
        
        return userInfo;
    }
}