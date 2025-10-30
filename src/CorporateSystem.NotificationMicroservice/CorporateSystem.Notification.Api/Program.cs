using CorporateSystem.Infrastructure.Extensions;
using CorporateSystem.Infrastructure.Repositories;
using CorporateSystem.Notification.Api;
using CorporateSystem.Notification.Api.GrpcServices;
using CorporateSystem.Services.Extensions;
using CorporateSystem.Services.Options;
using CorporateSystem.Services.Services.Implementations;
using Google.Api;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddOpenApi();
        builder.Services.AddControllers();
        builder.Services.AddNotificationInfrastructure();
        builder.Services.AddNotificationServices();
        builder.Services.AddGrpc(x =>
        {
            x.EnableDetailedErrors = true;
        }).AddJsonTranscoding();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(50051, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });
        builder.Services.AddGrpcReflection();

        builder.Services.Configure<FakeMailOptions>(builder.Configuration.GetSection("FakeMailOptions"));
        builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailOptions"));
        builder.Services.Configure<NotificationOptions>(builder.Configuration.GetSection("NotificationOptions"));

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(connectionString));
        
        var app = builder.Build();
        
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<DataContext>();
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
            }
        }
        
        if (app.Environment.IsDevelopment())
        {
            app.MapGrpcReflectionService();
            app.MapOpenApi(); 
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.MapGrpcService<GrpcMailService>();

        app.Run();
    }
}