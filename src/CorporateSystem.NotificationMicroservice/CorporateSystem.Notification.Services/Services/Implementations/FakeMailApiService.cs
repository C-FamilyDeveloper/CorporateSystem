using System.Net.Http.Json;
using CorporateSystem.Services.Dtos;
using CorporateSystem.Services.Options;
using CorporateSystem.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CorporateSystem.Services.Services.Implementations;

internal sealed class FakeMailApiService(IOptions<FakeMailOptions> fakeMailOptions, ILogger<FakeMailApiService> logger) 
    : IFakeMailApiService
{
    public async Task SendEmailMessageAsync(
        SendEmailMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var responseMessage = await SendAsync(
            new Uri(new Uri(fakeMailOptions.Value.ConnectionString), "/api/send-message"),
            HttpMethod.Post,
            request,
            cancellationToken: cancellationToken);
        
        logger.LogInformation($"{nameof(SendEmailMessageAsync)}: response status code={responseMessage.StatusCode}");
        responseMessage.EnsureSuccessStatusCode();
    }

    public async Task<GetEmailByTokenResponse> GetEmailByTokenAsync(
        GetEmailByTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var responseMessage = await SendAsync(
            new Uri(new Uri(fakeMailOptions.Value.ConnectionString), "/api/get-email-by-token"),
            HttpMethod.Get,
            headers: new() { {"X-Token", request.Token} },
            cancellationToken: cancellationToken);
        
        logger.LogInformation($"{nameof(GetEmailByTokenAsync)}: response status code={responseMessage.StatusCode}");
        responseMessage.EnsureSuccessStatusCode();
        
        return (await responseMessage.Content.ReadFromJsonAsync<GetEmailByTokenResponse>(cancellationToken))!;
    }

    private HttpClient CreateHttpClient()
    {
        return new HttpClient
        {
            BaseAddress = new Uri(fakeMailOptions.Value.ConnectionString)
        };
    }

    private async Task<HttpResponseMessage> SendAsync(
        Uri uri,
        HttpMethod httpMethod,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateHttpClient();
        
        var httpMessage = new HttpRequestMessage
        {
            RequestUri = uri,
            Method = httpMethod,
        };

        if (headers is not null)
        {
            foreach (var (header, value) in headers)
            {
                httpMessage.Headers.Add(header, value);
            }
        }

        return await httpClient.SendAsync(httpMessage, cancellationToken);
    }
    
    private async Task<HttpResponseMessage> SendAsync<T>(
        Uri uri,
        HttpMethod httpMethod,
        T? data = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        using var httpClient = CreateHttpClient();
        
        var httpMessage = new HttpRequestMessage
        {
            RequestUri = uri,
            Method = httpMethod,
        };

        if (data is not null)
        {
            httpMessage.Content = JsonContent.Create(data);
        }

        if (headers is not null)
        {
            foreach (var (header, value) in headers)
            {
                httpMessage.Headers.Add(header, value);
            }
        }

        return await httpClient.SendAsync(httpMessage, cancellationToken);
    }
}