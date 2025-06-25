using System.Security.Claims;
using System.Text;
using CorporateSystem.SharedDocs.Api.Backgrounds;
using CorporateSystem.SharedDocs.Api.Hubs;
using CorporateSystem.SharedDocs.Api.Middlewares;
using CorporateSystem.SharedDocs.Infrastructure.Extensions;
using CorporateSystem.SharedDocs.Infrastructure.Migrations;
using CorporateSystem.SharedDocs.Infrastructure.Options;
using CorporateSystem.SharedDocs.Kafka.Extensions;
using CorporateSystem.SharedDocs.Kafka.Models;
using CorporateSystem.SharedDocs.Kafka.Options;
using CorporateSystem.SharedDocs.Services.Extensions;
using CorporateSystem.SharedDocs.Services.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


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
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(10); 
            options.HandshakeTimeout = TimeSpan.FromSeconds(5);
        });
        services.AddSharedDocsInfrastructure();
        services.AddSharedDocsServices();
        services.AddKafkaConsumers();
        services.AddHttpContextAccessor();
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowApiGateway", builder =>
            {
                builder.WithOrigins("http://localhost:5000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Configuration.GetSection("JwtToken")["JwtSecret"]!)),
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero
            };
        });
        
        services.Configure<AuthMicroserviceOptions>(Configuration.GetSection("AuthMicroserviceOptions"));
        services.Configure<PostgresOptions>(Configuration.GetSection("PostgresOptions"));
        services.Configure<ConsumerOptions>(nameof(UserDeleteEvent), Configuration.GetSection(nameof(ConsumerOptions)));

        services.AddHostedService<KafkaBackgroundService>();
        
        var postgresConnectionString = Configuration.GetSection("PostgresOptions")["ConnectionString"]!;
        var migrationService = new Migrator(postgresConnectionString);
        migrationService.ApplyMigrations();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<UserInfoMiddleware>();
        app.UseHttpsRedirection();
        
        app.UseRouting();

        app.UseCors("AllowApiGateway");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseWebSockets();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<DocumentHub>("/document-hub");
            endpoints.MapControllers();
        });
    }
}