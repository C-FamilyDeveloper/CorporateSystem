using CorporateSystem.ApiGateway.Services.Services.Implementations;
using CorporateSystem.ApiGateway.Services.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateSystem.ApiGateway.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiGatewayServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IAuthService, AuthService>();
    } 
}