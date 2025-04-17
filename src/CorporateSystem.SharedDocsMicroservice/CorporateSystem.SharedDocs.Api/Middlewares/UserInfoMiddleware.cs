using System.Text.Json;
using CorporateSystem.SharedDocs.Api.Requests;

namespace CorporateSystem.SharedDocs.Api.Middlewares;

public class UserInfoMiddleware
{
    private readonly RequestDelegate _next;

    public UserInfoMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("UserInfo", out var userInfoHeader))
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
                Console.Error.WriteLine($"Failed to deserialize UserInfo: {ex.Message}");
            }
        }

        await _next(context);
    }
}