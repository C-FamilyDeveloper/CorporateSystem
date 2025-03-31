using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using CorporateSystem.Auth.Api.Dtos.Auth;
using CorporateSystem.Auth.Domain.Enums;
using CorporateSystem.Auth.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CorporateSystem.Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("sign-in")]
    public async Task<ActionResult<AuthResponse>> SignIn(
        [FromBody]AuthRequest request,
        [FromServices] IAuthService authService)
    {
        try
        {
            var token = await authService
                .AuthenticateAsync(new AuthUserDto(request.Email, request.Password));
            
            return new JsonResult(new AuthResponse
            {
                Token = token
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Неверный логин или пароль");
        }
        catch (Exception)
        {
            return Problem("Что то пошло не так");
        }
    }
    
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp(
        [FromBody] RegisterRequest request, 
        [FromServices] IRegistrationService registrationService)
    {
        try
        {
            await registrationService
                .RegisterAsync(
                    new RegisterUserDto(request.Email, request.Password, request.RepeatedPassword));
            
            return Ok("Регистрация прошла успешно");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("success-registration")]
    public async Task<IActionResult> SuccessRegistration(
        [FromBody] SuccessRegisterRequest request,
        [FromServices] IRegistrationService registrationService)
    {
        try
        {
            if (request is null)
                throw new Exception("Некорректный запрос");
            
            await registrationService
                .SuccessRegisterAsync(new SuccessRegisterUserDto(request.Email, request.Password, request.SuccessCode));

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("validate-token")]
    public ActionResult<UserInfo> ValidateToken(
        [FromBody]TokenValidationRequest request, 
        [FromServices] IAuthService authService)
    {
        try
        {
            var isValid = authService.ValidateToken(request.Token);

            if (!isValid)
            {
                return Unauthorized("Invalid token.");
            }
            
            var userInfo = GetUserInfoByToken(request.Token);
            return Ok(userInfo);
        }
        catch (Exception e)
        {
            return Unauthorized($"Token validation failed. {e.Message}");
        }
    }
    
    private UserInfo GetUserInfoByToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "nameid");
        var roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "role");
        
        if (userIdClaim == null || roleClaim == null)
        {
            throw new InvalidOperationException("Invalid token claims.");
        }

        return new UserInfo
        {
            Id = int.Parse(userIdClaim.Value),
            Role = roleClaim.Value
        };
    }
}