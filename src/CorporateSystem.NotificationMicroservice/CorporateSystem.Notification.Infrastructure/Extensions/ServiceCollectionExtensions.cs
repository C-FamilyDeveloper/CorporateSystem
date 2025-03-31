using CorporateSystem.Infrastructure.Repositories.Implementations;
using CorporateSystem.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateSystem.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services)
    {
        return services
            .AddScoped<IContextFactory, ContextFactory>();
    }
}