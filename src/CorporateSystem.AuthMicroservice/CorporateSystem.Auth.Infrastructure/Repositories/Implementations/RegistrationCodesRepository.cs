using System.Net;
using CorporateSystem.Auth.Domain.Exceptions;
using CorporateSystem.Auth.Infrastructure.Options;
using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CorporateSystem.Auth.Infrastructure.Repositories.Implementations;

internal class RegistrationCodesRepository(
    IOptions<RedisOptions> redisOptions,
    ILogger<RegistrationCodesRepository> logger)
    : RedisRepository(redisOptions.Value), IRegistrationCodesRepository
{
    protected override string KeyPrefix { get; } = "registration_codes";

    public async Task<int?> GetAsync(object[] identifiers, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        logger.LogInformation($"{nameof(GetAsync)}: Try getting code by key={string.Join(",", identifiers)}");
        var key = GetKey(identifiers);
        var database = await GetConnectionAsync(cancellationToken);
        var value = await database.StringGetAsync(key);

        logger.LogInformation($"{nameof(GetAsync)}: Get value={value}");
        
        if (value.IsNullOrEmpty)
            return null;

        if (!int.TryParse(value, out var result))
        {
            logger.LogError($"{nameof(GetAsync)}: Не удается преобразовать {value} в int");
            throw new ExceptionWithStatusCode("Что-то пошло не так", HttpStatusCode.BadRequest);
        }

        return result;
    }

    public async Task CreateAsync(object[] identifiers, int code, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var key = GetKey(identifiers);
        
        logger.LogInformation($"{nameof(CreateAsync)}: key={key}");
        
        var database = await GetConnectionAsync(cancellationToken);
        await database.StringSetAsync(key, code.ToString(), TimeSpan.FromMinutes(5));
        
        logger.LogInformation($"{nameof(CreateAsync)}: created key={key} with value={code}");
    }

    public async Task DeleteAsync(object[] identifiers, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var key = GetKey(identifiers);
        
        logger.LogInformation($"{nameof(DeleteAsync)}: key={key}");
        
        var database = await GetConnectionAsync(cancellationToken);
        await database.KeyDeleteAsync(key);
        
        logger.LogInformation($"{nameof(DeleteAsync)}: deleted key={key}");
    }
}