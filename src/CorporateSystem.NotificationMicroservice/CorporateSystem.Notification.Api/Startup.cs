using CorporateSystem.Infrastructure.Extensions;
using CorporateSystem.Infrastructure.Repositories;
using CorporateSystem.Notification.Api.GrpcServices;
using CorporateSystem.Services.Extensions;
using CorporateSystem.Services.Options;
using Microsoft.EntityFrameworkCore;

namespace CorporateSystem.Notification.Api;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Метод для регистрации сервисов
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddControllers();
        services.AddNotificationInfrastructure();
        services.AddNotificationServices();
        services.AddGrpc();

        services.Configure<FakeMailOptions>(_configuration.GetSection("FakeMailOptions"));

        services.AddHttpClient("FakeMailService");

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(connectionString));
    }

    // Метод для настройки HTTP-конвейера
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
        }

        app.UseHttpsRedirection(); // Перенаправление на HTTPS
        app.UseRouting(); // Добавляем маршрутизацию

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<GrpcMailService>(); // Регистрация gRPC-сервиса
        });
    }
}