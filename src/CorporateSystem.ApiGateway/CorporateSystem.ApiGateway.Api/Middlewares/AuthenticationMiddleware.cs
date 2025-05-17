using System.Text.Json;
using CorporateSystem.ApiGateway.Services.Services.Interfaces;
using Microsoft.Extensions.Primitives;

namespace CorporateSystem.ApiGateway.Api.Middlewares;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService, ILogger<AuthenticationMiddleware> logger)
    {
        try
        {
            logger.LogInformation($"{nameof(InvokeAsync)}: {context.Request.Path.ToString()}");
        
            if (RequiresAuthentication(context))
            {
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = context.Request.Query["access_token"];
                }

                logger.LogInformation($"{nameof(InvokeAsync)}: Token={token}");

                if (!string.IsNullOrWhiteSpace(token))
                {
                    var userInfo = await authService.GetUserInfoAsyncByToken(token, context.RequestAborted);
                    context.Request.Headers["X-User-Info"] = JsonSerializer.Serialize(userInfo);
                    context.Request.Headers["Authorization"] = $"Bearer {token}";
                    logger.LogInformation($"{nameof(InvokeAsync)}: UserInfo={userInfo}");
                }
                else
                {
                    throw new UnauthorizedAccessException("Token is missing");
                }
            }
        
            await _next(context);

            context.Request.Headers.Remove("X-User-Info");
        }
        catch (Exception e)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            logger.LogError($"{nameof(InvokeAsync)}: {e.Message}");
        }
    }

    private bool RequiresAuthentication(HttpContext context)
    {
        var path = context.Request.Path.ToString();
        return !path.StartsWith("/api/auth");
    }
}