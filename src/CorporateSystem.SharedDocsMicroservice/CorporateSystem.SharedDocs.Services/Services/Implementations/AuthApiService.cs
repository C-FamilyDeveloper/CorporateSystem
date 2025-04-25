using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using CorporateSystem.SharedDocs.Domain.Exceptions;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Options;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("CorporateSystem.SharedDocs.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace CorporateSystem.SharedDocs.Services.Services.Implementations;

internal class AuthApiService(
    IOptions<AuthMicroserviceOptions> authOptions,
    IHttpClientFactory httpClientFactory,
    ILogger<AuthApiService> logger) : IAuthApiService
{
    private readonly AuthMicroserviceOptions _authMicroserviceOptions = authOptions.Value;
    
    public async Task<string[]> GetUserEmailsByIdsAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(ids);

            using var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_authMicroserviceOptions.Host);

            var request = new GetUserEmailsByIdsRequest
            {
                UserIds = ids
            };

            var httpMessage = new HttpRequestMessage
            {
                Content = JsonContent.Create(request),
                RequestUri = new Uri(httpClient.BaseAddress, "/api/auth/get-user-emails-by-id"),
                Method = HttpMethod.Post
            };

            var response = await httpClient.SendAsync(httpMessage, cancellationToken);
            logger.LogInformation($"{nameof(GetUserEmailsByIdsAsync)}: Response status code: {response.StatusCode}");
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<GetUserEmailsByIdsResponse>(cancellationToken);

            if (data is null)
            {
                logger.LogError($"{nameof(GetUserEmailsByIdsAsync)}: data=null");
                throw new ExceptionWithStatusCode("Что-то пошло не так", HttpStatusCode.BadRequest);
            }
            
            return data.UserEmails;
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(GetUserEmailsByIdsAsync)}: {e.Message}");
            throw;
        }
    }

    public async Task<int[]> GetUserIdsByEmailsAsync(string[] emails, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(emails);

            using var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_authMicroserviceOptions.Host);

            var request = new GetUserIdsByEmailsRequest
            {
                UserEmails = emails
            };

            logger.LogInformation($"{nameof(GetUserIdsByEmailsAsync)}: emails={string.Join(",", emails)}");
            
            var httpMessage = new HttpRequestMessage
            {
                Content = JsonContent.Create(request),
                RequestUri = new Uri(httpClient.BaseAddress, "/api/auth/get-user-ids-by-email"),
                Method = HttpMethod.Post
            };

            logger.LogInformation($"{nameof(GetUserIdsByEmailsAsync)}: Request path={httpMessage.RequestUri.ToString()}");
            
            var response = await httpClient.SendAsync(httpMessage, cancellationToken);
            logger.LogInformation($"{nameof(GetUserIdsByEmailsAsync)}: Response status code: {response.StatusCode}");
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<GetUserIdsByEmailsResponse>(cancellationToken);

            if (data is null)
            {
                logger.LogError($"{nameof(GetUserIdsByEmailsAsync)}: data is null");
                throw new ExceptionWithStatusCode("Что-то пошло не так", HttpStatusCode.BadRequest);
            }
            
            return data.UserIds;
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(GetUserIdsByEmailsAsync)}: {e.Message}");
            throw;
        }
    }
}