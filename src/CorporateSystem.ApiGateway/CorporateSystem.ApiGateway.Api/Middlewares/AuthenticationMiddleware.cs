using CorporateSystem.ApiGateway.Services.Services.Interfaces;

namespace CorporateSystem.ApiGateway.Api.Middlewares;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        try
        {
            if (RequiresAuthentication(context))
            {
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userInfo = await authService.GetUserInfoAsyncByToken(token, context.RequestAborted);
                context.Items["UserInfo"] = userInfo;
            }
            
            await _next(context);
        }
        catch
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
        }
    }

    private bool RequiresAuthentication(HttpContext context)
    {
        var path = context.Request.Path.ToString();
        return !path.StartsWith("/api/auth");
    }
}