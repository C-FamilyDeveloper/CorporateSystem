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
        _logger.LogInformation($"{nameof(InvokeAsync)}: connectionId={context.Connection.Id}");
        _logger.LogInformation($"{nameof(InvokeAsync)}: route={context.Request.Path}");
        if (context.Request.Headers.TryGetValue("X-User-Info", out var userInfoHeader))
        {
            try
            {
                var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoHeader);
                if (userInfo != null)
                {
                    _logger.LogInformation($"UserInfo={userInfoHeader} added in context.Items");
                    context.Items["X-User-Info"] = userInfo;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"{nameof(InvokeAsync)}: Failed to deserialize UserInfo: {ex.Message}");
            }
        }
        else
        {
            _logger.LogInformation($"{nameof(InvokeAsync)}: Headers dont have X-User-Info");
        }

        if (context.Request.Headers.TryGetValue("Authorization", out var tokenHeader))
        {
            var token = tokenHeader.ToString().Replace("Bearer ", string.Empty);
            _logger.LogInformation($"{nameof(InvokeAsync)}: token={token}");
            context.Items["Authorization"] = token;
        }
        else
        {
            _logger.LogInformation($"{nameof(InvokeAsync)}: Header dont have Authorization");
        }
        
        await _next(context);
    }
}