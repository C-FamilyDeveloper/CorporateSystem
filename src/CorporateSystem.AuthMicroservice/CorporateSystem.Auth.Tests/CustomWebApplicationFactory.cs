using CorporateSystem.Auth.Services.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CorporateSystem.Auth.Tests;

public class CustomWebApplicationFactory<TEntryPoint> 
    : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
    public Mock<IAuthService> MockAuthService { get; } = new();
    public Mock<IRegistrationService> MockRegistrationService { get; } = new();
    public Mock<IUserService> MockUserService { get; } = new();
    public string TestSecretKey { get; set; }

    public CustomWebApplicationFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        TestSecretKey = configuration["JwtToken:JwtSecret"];
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Replace(new ServiceDescriptor(typeof(IUserService), MockUserService.Object));
            services.Replace(new ServiceDescriptor(typeof(IAuthService), MockAuthService.Object));
            services.Replace(new ServiceDescriptor(typeof(IRegistrationService), MockRegistrationService.Object));
        });
    }

    public void ResetMocks()
    {
        MockUserService.Invocations.Clear();
        MockAuthService.Invocations.Clear();
        MockRegistrationService.Invocations.Clear();
    }
}