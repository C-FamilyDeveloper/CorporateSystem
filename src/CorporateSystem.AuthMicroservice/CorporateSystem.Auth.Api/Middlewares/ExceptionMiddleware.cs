using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using CorporateSystem.Auth.Services.Exceptions;

namespace CorporateSystem.Auth.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        { 
            await _next(context);
        }
        catch (Exception e)
        {
            _logger.LogError($"{nameof(ExceptionMiddleware)}: {e.Message}");
            await HandleExceptionAsync(context, e);
        }
    }
    
    private Task HandleExceptionAsync(HttpContext context, Exception e)
    {
        var code = HttpStatusCode.InternalServerError;
        var message = "Что-то пошло не так";
        switch (e)
        {
            case InvalidRegistrationException exception:
                code = HttpStatusCode.BadRequest;
                message = JsonSerializer.Serialize(exception.Message);
                break;
            case InvalidAuthorizationException exception:
                code = HttpStatusCode.Forbidden;
                message = JsonSerializer.Serialize(exception.Message);
                break;
            case UserNotFoundException exception:
                code = HttpStatusCode.NotFound;
                message = JsonSerializer.Serialize(exception.Message);
                break;
            case RefreshTokenExpiredException exception:
                code = HttpStatusCode.Forbidden;
                break;
            case ArgumentOutOfRangeException exception:
                code = HttpStatusCode.BadRequest;
                message = JsonSerializer.Serialize(exception.Message);
                break;
            case ArgumentException exception:
                code = HttpStatusCode.BadRequest;
                message = JsonSerializer.Serialize(exception.Message);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(message);
    }
}