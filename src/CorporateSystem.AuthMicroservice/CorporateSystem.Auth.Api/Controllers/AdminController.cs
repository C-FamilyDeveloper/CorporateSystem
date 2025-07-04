using CorporateSystem.Auth.Api.Dtos.Auth;
using CorporateSystem.Auth.Domain.Enums;
using CorporateSystem.Auth.Infrastructure;
using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using CorporateSystem.Auth.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CorporateSystem.Auth.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(Role.Admin))]
[Route("api/auth/admin")]
public class AdminController : ControllerBase
{
    [HttpPost("users")]
    public async Task<IActionResult> GetUsers(
        [FromBody] GetUsersRequest request,
        [FromServices] IContextFactory<DataContext> contextFactory)
    {
        var users = await contextFactory.ExecuteWithoutCommitAsync(context => context.Users
            .Skip((request.Page - 1) * request.PerPage)
            .Take(request.PerPage)
            .ToArrayAsync());

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
    
    [HttpPost("users/{userId:int}")]
    public async Task<IActionResult> DeleteUser(
        [FromBody] DeleteUsersRequest request,
        [FromServices] IUserService userService)
    {
        await userService.DeleteUsersAsync(request.UserIds);

        return Ok();
    }
}