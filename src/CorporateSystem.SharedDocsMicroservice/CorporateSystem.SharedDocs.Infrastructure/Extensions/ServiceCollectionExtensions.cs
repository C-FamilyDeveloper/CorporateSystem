using CorporateSystem.SharedDocs.Infrastructure.Repositories.Implementations;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateSystem.SharedDocs.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedDocsInfrastructure(this IServiceCollection services)
    {
        return services
            .AddScoped<IDocumentUserRepository, DocumentUserRepository>()
            .AddScoped<IDocumentRepository, DocumentRepository>();
    }
}