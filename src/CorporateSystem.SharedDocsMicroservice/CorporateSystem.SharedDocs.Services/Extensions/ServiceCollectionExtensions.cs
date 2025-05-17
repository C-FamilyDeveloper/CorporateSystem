using CorporateSystem.SharedDocs.Services.Handlers;
using CorporateSystem.SharedDocs.Services.Services.Implementations;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateSystem.SharedDocs.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedDocsServices(this IServiceCollection services)
    {
        services
            .AddScoped<AuthHeaderHandler>()
            .AddHttpClient<IAuthApiService, AuthApiService>()
            .AddHttpMessageHandler<AuthHeaderHandler>();
        
        return services
            .AddScoped<IDocumentService, DocumentService>()
            .AddScoped<IDocumentChangeLogService, DocumentChangeLogService>();
    }
}