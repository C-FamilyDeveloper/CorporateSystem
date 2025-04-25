using System.Text.Json;
using CorporateSystem.SharedDocs.Api.Requests;

namespace CorporateSystem.SharedDocs.Api.Middlewares;

public class UserInfoMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserInfoMiddleware> _logger;

    public UserInfoMiddleware(RequestDelegate next, ILogger<UserInfoMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-User-Info", out var userInfoHeader))
        {
            try
            {
                var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoHeader);
                if (userInfo != null)
                {
                    context.Items["UserInfo"] = userInfo;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"{nameof(InvokeAsync)}: Failed to deserialize UserInfo: {ex.Message}");
            }
        }

        await _next(context);
    }
}