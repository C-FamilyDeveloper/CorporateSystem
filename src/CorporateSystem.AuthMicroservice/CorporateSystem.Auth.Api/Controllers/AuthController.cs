using System.IdentityModel.Tokens.Jwt;
using CorporateSystem.Auth.Api.Dtos.Auth;
using CorporateSystem.Auth.Domain.Exceptions;
using CorporateSystem.Auth.Services.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CorporateSystem.Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn(
        [FromBody]AuthRequest request,
        [FromServices] IAuthService authService)
    {
        try
        {
            logger.LogInformation($"{nameof(SignIn)}: connectionId={HttpContext.Connection.Id}");
            
            var token = await authService
                .AuthenticateAsync(new AuthUserDto(request.Email, request.Password));
            
            return Ok(new AuthResponse
            {
                Token = token
            });
        }
        catch (ExceptionWithStatusCode e)
        {
            logger.LogError($"{nameof(SignIn)}: {e.Message}");
            return StatusCode((int)e.StatusCode, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(SignIn)}: {e.Message}");
            return StatusCode(StatusCodes.Status400BadRequest, "Что-то пошло не так");
        }
    }
    
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp(
        [FromBody] RegisterRequest request, 
        [FromServices] IRegistrationService registrationService)
    {
        try
        {
            logger.LogInformation($"{nameof(SignUp)}: connectionId={HttpContext.Connection.Id}");
            
            await registrationService
                .RegisterAsync(
                    new RegisterUserDto(request.Email, request.Password, request.RepeatedPassword));

            return Ok();
        }
        catch (ExceptionWithStatusCode e)
        {
            logger.LogError($"{nameof(SignUp)}: {e.Message}");
            return StatusCode((int)e.StatusCode, e.Message);
        }
        catch (Exception ex)
        {
            logger.LogError($"{nameof(SignUp)}: {ex.Message}");
            return StatusCode(StatusCodes.Status400BadRequest, "Что-то пошло не так");
        }
    }

    [HttpPost("success-registration")]
    public async Task<IActionResult> SuccessRegistration(
        [FromBody] SuccessRegisterRequest request,
        [FromServices] IRegistrationService registrationService)
    {
        try
        {
            logger.LogInformation($"{nameof(SuccessRegistration)}: connectionId={HttpContext.Connection.Id}");
            
            if (request is null)
            {
                logger.LogError($"{nameof(SuccessRegistration)}: request=null");
                throw new Exception("Некорректный запрос");
            }

            await registrationService
                .SuccessRegisterAsync(new SuccessRegisterUserDto(request.Email, request.Password, request.SuccessCode));

            return Ok();
        }
        catch (ExceptionWithStatusCode e)
        {
            logger.LogError($"{nameof(SuccessRegistration)}: {e.Message}");
            return StatusCode((int)e.StatusCode, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(SuccessRegistration)}: {e.Message}");
            return StatusCode(StatusCodes.Status400BadRequest, "Что-то пошло не так");
        }
    }
    
    [HttpPost("validate-token")]
    public IActionResult ValidateToken(
        [FromBody]TokenValidationRequest request, 
        [FromServices] IAuthService authService)
    {
        try
        {
            logger.LogInformation($"{nameof(ValidateToken)}: connectionId={HttpContext.Connection.Id}");
            
            var isValid = authService.ValidateToken(request.Token);
            
            if (!isValid)
            {
                logger.LogError($"{nameof(ValidateToken)}: Token={request.Token} is not valid");
                return Unauthorized("Invalid token.");
            }

            var userInfo = GetUserInfoByToken(request.Token);
            return Ok(userInfo);
        }
        catch (ExceptionWithStatusCode e)
        {
            logger.LogError($"{nameof(ValidateToken)}: {e.Message}");
            return StatusCode((int)e.StatusCode, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(ValidateToken)}: {e.Message}");
            return StatusCode(StatusCodes.Status401Unauthorized, "Token validation failed.");
        }
    }

    [HttpPost("get-user-emails-by-id")]
    public async Task<IActionResult> GetUserEmailsById(
        [FromBody] GetUserEmailsByIdsRequest request,
        [FromServices] IUserService userService)
    {
        try
        {
            logger.LogInformation($"{nameof(GetUserEmailsById)}: connectionId={HttpContext.Connection.Id}");
            
            var users = await userService.GetUsersByIdsAsync(request.UserIds);
            var emails = users.Select(user => user.Email).ToArray();

            logger.LogInformation($"{nameof(GetUserEmailsById)}: emails={string.Join(",", emails)}");
            
            return Ok(new GetUserEmailsByIdsResponse
            {
                UserEmails = emails
            });
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(GetUserEmailsById)}: {e.Message}");
            return StatusCode(StatusCodes.Status400BadRequest, "Что-то пошло не так");
        }
    }

    [HttpPost("get-user-ids-by-email")]
    public async Task<IActionResult> GetUserIdsByEmail(
        [FromBody] GetUserIdsByEmailsRequest request,
        [FromServices] IUserService userService)
    {
        try
        {
            logger.LogInformation($"{nameof(GetUserIdsByEmail)}: connectionId={HttpContext.Connection.Id}");
            
            var users = await userService.GetUsersByEmailsAsync(request.UserEmails);
            var ids = users.Select(user => user.Id).ToArray();
            
            logger.LogInformation($"{nameof(GetUserIdsByEmail)}: ids={string.Join(",", ids)}");
            
            return Ok(new GetUserIdsByEmailsResponse
            {
                UserIds = ids
            });
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(GetUserIdsByEmail)}: {e.Message}");
            return StatusCode(StatusCodes.Status400BadRequest, "Что-то пошло не так");
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
            logger.LogError($"{nameof(GetUserInfoByToken)}: Token={token}, userIdClaim={userIdClaim?.Value} roleClaim={roleClaim?.Value}");
            throw new InvalidOperationException("Invalid token claims.");
        }

        return new UserInfo
        {
            Id = int.Parse(userIdClaim.Value),
            Role = roleClaim.Value
        };
    }
}