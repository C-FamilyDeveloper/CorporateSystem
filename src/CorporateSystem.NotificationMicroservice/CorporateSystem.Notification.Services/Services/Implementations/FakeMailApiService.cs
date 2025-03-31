using System.Net.Http.Json;
using System.Text.Json;
using CorporateSystem.Services.Dtos;
using CorporateSystem.Services.Options;
using CorporateSystem.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CorporateSystem.Services.Services.Implementations;

internal class FakeMailApiService(IOptions<FakeMailOptions> fakeMailOptions) 
    : IFakeMailApiService
{
    public async Task SendEmailMessageAsync(
        SendEmailMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateHttpClient();
        
        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(httpClient.BaseAddress!, "/api/send-message"),
            Content = JsonContent.Create(request)
        };

        var responseMessage = await httpClient.SendAsync(requestMessage, cancellationToken);
        responseMessage.EnsureSuccessStatusCode();
    }

    public async Task<GetEmailByTokenResponse> GetEmailByTokenAsync(
        GetEmailByTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateHttpClient();
        
        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            Headers = { {"X-Token", request.Token} },
            RequestUri = new Uri(httpClient.BaseAddress!, "/api/get-email-by-token")
        };

        var responseMessage = await httpClient.SendAsync(requestMessage, cancellationToken);
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
}