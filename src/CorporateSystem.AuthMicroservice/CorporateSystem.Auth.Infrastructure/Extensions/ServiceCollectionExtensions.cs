using CorporateSystem.Auth.Infrastructure.Options;
using CorporateSystem.Auth.Infrastructure.Repositories.Implementations;
using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CorporateSystem.Auth.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services)
    {
        return services
            .AddScoped<IContextFactory, ContextFactory>()
            .AddScoped<IRegistrationCodesRepository, RegistrationCodesRepository>()
            .AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisOptions = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
                return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
            });
    }
}