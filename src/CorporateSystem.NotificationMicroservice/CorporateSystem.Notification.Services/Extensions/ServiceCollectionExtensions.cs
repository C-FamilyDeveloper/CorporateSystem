using CorporateSystem.Services.Services.Factory;
using CorporateSystem.Services.Services.Implementations;
using CorporateSystem.Services.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateSystem.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IEmailSenderService, EmailSenderService>()
            .AddScoped<IMailService, MailService>()
            .AddScoped<IFakeMailService, FakeMailService>()
            .AddScoped<IFakeMailApiService, FakeMailApiService>()
            .AddSingleton<IEmailServiceFactory, EmailServiceFactory>();
    }
}