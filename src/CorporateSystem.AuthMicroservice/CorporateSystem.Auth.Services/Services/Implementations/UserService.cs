using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using CorporateSystem.Auth.Domain.Entities;
using CorporateSystem.Auth.Domain.Enums;
using CorporateSystem.Auth.Domain.Exceptions;
using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using CorporateSystem.Auth.Services.Extensions;
using CorporateSystem.Auth.Services.Options;
using CorporateSystem.Auth.Services.Services.Filters;
using CorporateSystem.Auth.Services.Services.GrpcServices;
using CorporateSystem.Auth.Services.Services.Interfaces;
using Grpc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

[assembly:InternalsVisibleTo("CorporateSystem.Auth.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace CorporateSystem.Auth.Services.Services.Implementations;

internal class UserService(
    IContextFactory contextFactory,
    IPasswordHasher passwordHasher,
    IRegistrationCodesRepository registrationCodesRepository,
    IUserRepository userRepository,
    GrpcNotificationClient grpcNotificationClient,
    IOptions<JwtToken> jwtTokenOptions,
    IOptions<NotificationOptions> notificationOptions,
    ILogger<UserService> logger) : IAuthService, IRegistrationService, IUserService
{
    private readonly JwtToken _jwtToken = jwtTokenOptions.Value;
    private readonly NotificationOptions _notificationOptions = notificationOptions.Value;
    
    public async Task<string> AuthenticateAsync(AuthUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await contextFactory.ExecuteWithoutCommitAsync(async context =>
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
        }, cancellationToken: cancellationToken);

        if (user == null || !passwordHasher.VerifyPassword(dto.Password, user.Password))
        {
            logger.LogInformation(user is null
                ? $"{nameof(AuthenticateAsync)}: Пользователь с email={dto.Email} не найден"
                : $"{nameof(AuthenticateAsync)}: email={dto.Email}, actual_password={dto.Password}, expected_password={user.Password}");
            
            throw new ExceptionWithStatusCode("Неправильный логин или пароль", HttpStatusCode.Unauthorized);
        }
        
        return GenerateJwtToken(user);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtToken.JwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            return validatedToken != null;
        }
        catch
        {
            return false;
        }
    }
    
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtToken.JwtSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Password != dto.RepeatedPassword)
        {
            logger.LogInformation($"{nameof(RegisterAsync)}: password={dto.Password}, repeated password={dto.RepeatedPassword}");
            throw new ExceptionWithStatusCode("Пароли не совпадают", HttpStatusCode.BadRequest);
        }

        var existingUser = await userRepository.GetUserByEmailAsync(dto.Email, cancellationToken);

        if (existingUser is not null)
        {
            logger.LogInformation($"{nameof(RegisterAsync)}: User с email={dto.Email} уже существует");
            throw new ExceptionWithStatusCode("Данная почта уже занята", HttpStatusCode.BadRequest);
        }

        if (await registrationCodesRepository.GetAsync([dto.Email], cancellationToken) != null)
        {
            logger.LogInformation($"{nameof(RegisterAsync)}: ключ с {dto.Email} уже сущесвует");
            throw new ExceptionWithStatusCode(
                "Вам на почту уже отправлен код. Попробуйте получить новый позже.",
                HttpStatusCode.BadRequest);
        }
        
        var code = Random.Shared.Next(100_000, 1_000_000);
        
        logger.LogInformation($"{nameof(RegisterAsync)}: Created code: {code}");
        
        await registrationCodesRepository.CreateAsync([dto.Email], code, cancellationToken);
        
        logger.LogInformation($"{nameof(RegisterAsync)}: Code {code} was written in redis");
        logger.LogInformation($"{nameof(RegisterAsync)}: Writing message to notification microservice");
        
        await grpcNotificationClient.SendMessageAsync(new SendMessageRequest
        {
            Title = "Регистрация в CorporateSystem",
            Message = $"Ваш код подтверждения: {code}",
            ReceiverEmails = { dto.Email },
            Token = _notificationOptions.Token
        }, cancellationToken);
        
        logger.LogInformation($"{nameof(RegisterAsync)}: Sending to notification is completed");
    }

    public async Task SuccessRegisterAsync(SuccessRegisterUserDto dto, CancellationToken cancellationToken = default)
    {
        dto.ShouldBeValid(logger);

        if (await registrationCodesRepository.GetAsync([dto.Email], cancellationToken) is null)
        {
            throw new ExceptionWithStatusCode("Неверный код", HttpStatusCode.BadRequest);
        }

        await registrationCodesRepository.DeleteAsync([dto.Email], cancellationToken);
        
        var addUserDto = new AddUserDto
        {
            Email = dto.Email,
            Password = passwordHasher.HashPassword(dto.Password),
            Role = Role.User
        };

        await userRepository.AddUserAsync(addUserDto, cancellationToken);
    }

    public Task<User[]> GetUsersByFilterAsync(UserFilter filter, CancellationToken cancellationToken = default)
    {
        return contextFactory.ExecuteWithoutCommitAsync(async context =>
        {
            var users = context.Users.AsQueryable();

            if (filter.Ids.IsNotNullAndNotEmpty())
            {
                users = users.Where(user => filter.Ids!.Contains(user.Id));
            }

            if (filter.Passwords.IsNotNullAndNotEmpty())
            {
                users = users.Where(user => filter.Passwords!.Contains(user.Password));
            }

            if (filter.Emails.IsNotNullAndNotEmpty())
            {
                users = users.Where(user => filter.Emails!.Contains(user.Email));
            }

            if (filter.Roles.IsNotNullAndNotEmpty())
            {
                users = users.Where(user => filter.Roles!.Contains(user.Role));
            }

            return await users.ToArrayAsync(cancellationToken);
        }, cancellationToken: cancellationToken);
    }
}