using System.Net;
using CorporateSystem.Auth.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CorporateSystem.Auth.Api.Extensions;

internal static class ExceptionHandlerExtensions
{
    public static IActionResult Handle(this ExceptionWithStatusCode e) => e.StatusCode switch
    {
        HttpStatusCode.Unauthorized => new UnauthorizedObjectResult(e.Message),
        HttpStatusCode.NotFound => new NotFoundObjectResult(e.Message),
        HttpStatusCode.BadRequest => new BadRequestObjectResult(e.Message),
        _ => new BadRequestObjectResult("Что-то пошло не так")
    };
}