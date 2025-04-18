using CorporateSystem.SharedDocs.Services.Services.Implementations;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateSystem.SharedDocs.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedDocsServices(this IServiceCollection services)
    {
        return services
            .AddHttpClient()
            .AddScoped<IDocumentService, DocumentService>()
            .AddScoped<IAuthApiService, AuthApiService>();
    }
}