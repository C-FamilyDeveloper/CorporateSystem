using System.Net;
using System.Text.Json;
using CorporateSystem.SharedDocs.Services.Exceptions;

namespace CorporateSystem.SharedDocs.Api.Middlewares;

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
            _logger.LogError($"{nameof(InvokeAsync)}: {e.Message}");
            await HandleExceptionAsync(context, e);
        }
    }
    
    private Task HandleExceptionAsync(HttpContext context, Exception e)
    {
        var code = HttpStatusCode.InternalServerError;
        var message = "Что-то пошло не так";
        switch (e)
        {
            case UnauthorizedAccessException exception:
                code = HttpStatusCode.Forbidden;
                message = JsonSerializer.Serialize(exception.Message);
                break;
            case InsufficientPermissionsException exception:
                code = HttpStatusCode.Forbidden;
                message = JsonSerializer.Serialize(exception.Message);
                break;
            case UserAlreadyExistException exception:
                code = HttpStatusCode.BadRequest;
                message = JsonSerializer.Serialize(exception.Message);
                break;
            case FileNotFoundException exception:
                code = HttpStatusCode.NotFound;
                message = JsonSerializer.Serialize(exception.Message);
                break;
            case ArgumentOutOfRangeException exception:
                code = HttpStatusCode.BadRequest;
                message = JsonSerializer.Serialize(exception.Message);
                break;
            case ArgumentNullException:
                code = HttpStatusCode.BadRequest;
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