using CorporateSystem.SharedDocs.Api.Hubs;
using CorporateSystem.SharedDocs.Api.Middlewares;
using CorporateSystem.SharedDocs.Infrastructure.Extensions;
using CorporateSystem.SharedDocs.Infrastructure.Migrations;
using CorporateSystem.SharedDocs.Infrastructure.Options;
using CorporateSystem.SharedDocs.Services.Extensions;
using CorporateSystem.SharedDocs.Services.Options;


namespace CorporateSystem.SharedDocs.Api;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddControllers();
        services.AddSignalR();
        services.AddSharedDocsInfrastructure();
        services.AddSharedDocsServices();
        
        services.Configure<AuthMicroserviceOptions>(Configuration.GetSection("AuthMicroserviceOptions"));
        services.Configure<PostgresOptions>(Configuration.GetSection("PostgresOptions"));
        
        var postgresConnectionString = Configuration.GetSection("PostgresOptions")["ConnectionString"]!;
        var migrationService = new Migrator(postgresConnectionString);
        migrationService.ApplyMigrations();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<UserInfoMiddleware>();
        app.UseHttpsRedirection();
        
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<DocumentHub>("/document-hub");
            endpoints.MapControllers();
        });
    }
}