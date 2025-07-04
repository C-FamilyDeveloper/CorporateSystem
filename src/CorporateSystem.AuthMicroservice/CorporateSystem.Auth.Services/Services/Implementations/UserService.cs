using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Confluent.Kafka;
using CorporateSystem.Auth.Domain.Entities;
using CorporateSystem.Auth.Domain.Enums;
using CorporateSystem.Auth.Infrastructure;
using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using CorporateSystem.Auth.Kafka;
using CorporateSystem.Auth.Kafka.Models;
using CorporateSystem.Auth.Services.Exceptions;
using CorporateSystem.Auth.Services.Extensions;
using CorporateSystem.Auth.Services.Options;
using CorporateSystem.Auth.Services.Services.GrpcServices;
using CorporateSystem.Auth.Services.Services.Interfaces;
using Grpc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("CorporateSystem.Auth.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CorporateSystem.Auth.Services.Services.Implementations;

internal sealed class UserService(
    IContextFactory<DataContext> contextFactory,
    IPasswordHasher passwordHasher,
    IRegistrationCodesRepository registrationCodesRepository,
    GrpcNotificationClient grpcNotificationClient,
    ITokenService tokenService,
    IOptions<NotificationOptions> notificationOptions,
    IKafkaAsyncProducer<Null, UserDeleteEvent> kafkaAsyncProducer,
    ILogger<UserService> logger) : IAuthService, IRegistrationService, IUserService
{
    private readonly NotificationOptions _notificationOptions = notificationOptions.Value;

    public Task<AuthResultDto> AuthenticateAsync(AuthUserDto dto, CancellationToken cancellationToken = default)
    {
        return contextFactory.ExecuteWithCommitAsync(async context =>
        {
            var user = await context.Users
                .Include(user => user.RefreshTokens)
                .FirstOrDefaultAsync(user => user.Email == dto.Email, cancellationToken);

            if (user is null || !passwordHasher.VerifyPassword(dto.Password, user.Password))
            {
                logger.LogInformation(user is null
                    ? $"{nameof(AuthenticateAsync)}: Пользователь с email={dto.Email} не найден"
                    : $"{nameof(AuthenticateAsync)}: email={dto.Email}, actual_password={dto.Password}, expected_password={user.Password}");

                throw new InvalidAuthorizationException("Неправильный логин или пароль");
            }

            var tokens = await Task.WhenAll(
                tokenService.GenerateJwtTokenAsync(user, cancellationToken),
                tokenService.GenerateRefreshTokenAsync(new GenerateRefreshTokenDto(user.Id, dto.UserIpAddress),
                    cancellationToken));

            var jwtToken = tokens[0];
            var refreshToken = tokens[1];
            
            var now = DateTimeOffset.UtcNow;
            
            var existingToken = user.RefreshTokens
                .FirstOrDefault(rt => rt.IpAddress == dto.UserIpAddress && rt.ExpiryOn > now);

            if (existingToken != null)
            {
                existingToken.Token = refreshToken;
                existingToken.ExpiryOn = now.AddDays(30);
            }
            else
            {
                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    IpAddress = dto.UserIpAddress,
                    CreatedAt = now,
                    ExpiryOn = now.AddDays(30),
                    UserId = user.Id,
                    User = user
                });
            }
            
            return new AuthResultDto(jwtToken);
        }, cancellationToken: cancellationToken);
    }

    public async Task RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default)
    {
        dto.ShouldBeValid(logger);

        var existingUser = await GetUserByEmailAsync(dto.Email, cancellationToken);

        if (existingUser is not null)
        {
            logger.LogInformation($"{nameof(RegisterAsync)}: User с email={dto.Email} уже существует");
            throw new InvalidRegistrationException("Данная почта уже занята");
        }

        if (await registrationCodesRepository.GetAsync([dto.Email], cancellationToken) != null)
        {
            logger.LogInformation($"{nameof(RegisterAsync)}: ключ с {dto.Email} уже сущесвует");
            throw new InvalidRegistrationException("Вам на почту уже отправлен код. Попробуйте получить новый позже.");
        }

        var code = Random.Shared.Next(100_000, 1_000_000);

        logger.LogInformation($"{nameof(RegisterAsync)}: Created code: {code}");

        await registrationCodesRepository.CreateAsync([dto.Email], code, cancellationToken);

        logger.LogInformation($"{nameof(RegisterAsync)}: Code {code} was written in redis");
        logger.LogInformation($"{nameof(RegisterAsync)}: Writing message to notification microservice");

        try
        {
            await grpcNotificationClient.SendMessageAsync(new SendMessageRequest
            {
                Title = "Регистрация в CorporateSystem",
                Message = $"Ваш код подтверждения: {code}",
                ReceiverEmails = { dto.Email },
                Token = _notificationOptions.Token
            }, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(RegisterAsync)}: {e.Message}");

            // Обвалилась отправка в мкс Notification, но запись в БД redis прошла успешно,
            // поэтому нужно откатить эту запись
            await registrationCodesRepository.DeleteAsync([dto.Email, code], cancellationToken);

            throw;
        }

        logger.LogInformation($"{nameof(RegisterAsync)}: Sending to notification is completed");
    }

    public async Task SuccessRegisterAsync(SuccessRegisterUserDto dto, CancellationToken cancellationToken = default)
    {
        dto.ShouldBeValid(logger);

        var code = await registrationCodesRepository.GetAsync([dto.Email], cancellationToken);

        if (code is null || code != dto.SuccessCode)
        {
            throw new InvalidRegistrationException("Неверный код");
        }

        await contextFactory.ExecuteWithCommitAsync(async context =>
        {
            await context.Users.AddAsync(new User
            {
                Email = dto.Email,
                Password = passwordHasher.HashPassword(dto.Password),
                Role = Role.User,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Gender = dto.Gender
            }, cancellationToken);
        }, cancellationToken: cancellationToken);
        
        await registrationCodesRepository.DeleteAsync([dto.Email], cancellationToken);
    }

    public Task<User[]> GetUsersByExpressionAsync(
        Expression<Func<User, bool>> expression,
        CancellationToken cancellationToken = default) =>
            contextFactory.ExecuteWithoutCommitAsync(async context =>
                    await context.Users
                        .Where(expression)
                        .ToArrayAsync(cancellationToken),
                cancellationToken: cancellationToken);

    public Task DeleteUsersAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        return contextFactory.ExecuteWithCommitAsync(async context =>
        {
            var currentUsers = await context.Users
                .Where(user => ids.Contains(user.Id))
                .ToArrayAsync(cancellationToken);

            if (!currentUsers.Any())
            {
                throw new UserNotFoundException("Ни один пользователь не найден");
            }

            var userIds = currentUsers.Select(user => user.Id).ToArray();
            
            if (currentUsers.Length != ids.Length)
            {
                var exceptIds = ids.Except(userIds).ToArray();

                var message = $"Не найдены пользователи с идентификаторами {string.Join(",", exceptIds)}";

                throw new UserNotFoundException(message);
            }

            context.Users.RemoveRange(currentUsers);

            var outboxEvents = userIds.Select(userId => new OutboxEvent
            {
                EventType = nameof(UserDeleteEvent),
                Payload = JsonSerializer.Serialize(new UserDeleteEvent
                {
                    UserId = userId
                })
            });
            
            context.OutboxEvents.AddRange(outboxEvents);
        }, cancellationToken: cancellationToken);
    }

    private Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken) =>
        contextFactory.ExecuteWithoutCommitAsync(
            async context =>
                await context.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken),
            cancellationToken: cancellationToken);
}