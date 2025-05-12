using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace CorporateSystem.SharedDocs.Services.Handlers;

internal class AuthHeaderHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
            .ToString()
            .Replace("Bearer ", string.Empty);
        
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}