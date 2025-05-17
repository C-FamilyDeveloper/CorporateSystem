using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Options;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("CorporateSystem.SharedDocs.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace CorporateSystem.SharedDocs.Services.Services.Implementations;

internal class AuthApiService : IAuthApiService
{
    private readonly ILogger<AuthApiService> _logger;
    private readonly HttpClient _httpClient;

    public AuthApiService(
        IOptions<AuthMicroserviceOptions> authOptions,
        HttpClient httpClient,
        ILogger<AuthApiService> logger)
    {
        var authMicroserviceOptions = authOptions.Value;
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(authMicroserviceOptions.Host);
    }
    
    public async Task<string[]> GetUserEmailsByIdsAsync(
        int[] ids,
        string? jwtToken =null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);
            
        var request = new GetUserEmailsByIdsRequest
        {
            UserIds = ids
        };

        var httpMessage = new HttpRequestMessage
        {
            Content = JsonContent.Create(request),
            RequestUri = new Uri(_httpClient.BaseAddress!, "/api/auth/get-user-emails-by-id"),
            Method = HttpMethod.Post
        };

        if (jwtToken is not null)
        {
            httpMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        var response = await _httpClient.SendAsync(httpMessage, cancellationToken);
        _logger.LogInformation($"{nameof(GetUserEmailsByIdsAsync)}: Response status code: {response.StatusCode}");
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<GetUserEmailsByIdsResponse>(cancellationToken);

        if (data is null)
        {
            _logger.LogError($"{nameof(GetUserEmailsByIdsAsync)}: data=null");
            throw new ArgumentException(nameof(data));
        }
            
        return data.UserEmails;
    }

    public async Task<int[]> GetUserIdsByEmailsAsync(string[] emails, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(emails);

        var request = new GetUserIdsByEmailsRequest
        {
            UserEmails = emails
        };

        _logger.LogInformation($"{nameof(GetUserIdsByEmailsAsync)}: emails={string.Join(",", emails)}");
            
        var httpMessage = new HttpRequestMessage
        {
            Content = JsonContent.Create(request),
            RequestUri = new Uri(_httpClient.BaseAddress!, "/api/auth/get-user-ids-by-email"),
            Method = HttpMethod.Post
        };

        _logger.LogInformation($"{nameof(GetUserIdsByEmailsAsync)}: Request path={httpMessage.RequestUri.ToString()}");
            
        var response = await _httpClient.SendAsync(httpMessage, cancellationToken);
        _logger.LogInformation($"{nameof(GetUserIdsByEmailsAsync)}: Response status code: {response.StatusCode}");
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<GetUserIdsByEmailsResponse>(cancellationToken);

        if (data is null)
        {
            _logger.LogError($"{nameof(GetUserIdsByEmailsAsync)}: data is null");
            throw new ArgumentNullException(nameof(data));
        }
            
        return data.UserIds;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}