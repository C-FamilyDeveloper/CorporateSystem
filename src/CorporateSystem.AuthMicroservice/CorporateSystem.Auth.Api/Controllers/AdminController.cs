using CorporateSystem.Auth.Api.Dtos.Auth;
using CorporateSystem.Auth.Domain.Enums;
using CorporateSystem.Auth.Services.Services.Filters;
using CorporateSystem.Auth.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorporateSystem.Auth.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(Role.Admin))]
[Route("api/auth/admin")]
public class AdminController : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromServices] IUserService userService)
    {
        var users = await userService.GetUsersByFilterAsync(new UserFilter());

        var response = users.Select(user => new GetUsersResponse
        {
            Email = user.Email,
            Gender = user.Gender,
            Role = user.Role,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Id = user.Id
        }).ToArray();

        return Ok(response);
    }
    
    [HttpDelete("users/{userId:int}")]
    public async Task<IActionResult> DeleteUser(
        [FromRoute] int userId,
        [FromServices] IUserService userService)
    {
        await userService.DeleteUsersAsync([userId]);

        return Ok();
    }
}