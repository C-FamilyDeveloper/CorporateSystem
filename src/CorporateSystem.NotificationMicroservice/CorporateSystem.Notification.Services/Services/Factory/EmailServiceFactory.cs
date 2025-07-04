using CorporateSystem.Services.Services.Implementations;
using CorporateSystem.Services.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateSystem.Services.Services.Factory;

public interface IEmailServiceFactory
{
    IMailService Build(string receiverEmail);
}

internal sealed class EmailServiceFactory(IServiceProvider serviceProvider) : IEmailServiceFactory
{
    public IMailService Build(string receiverEmail) => GetDomain(receiverEmail) switch
    {
        "bobr.ru" => serviceProvider.GetRequiredService<IFakeMailService>(),
        _ => serviceProvider.GetRequiredService<IMailService>()
    };

    private static string GetDomain(string email)
    {
        var data = email.Split('@');
        
        ArgumentOutOfRangeException.ThrowIfNotEqual(data.Length, 2);

        return data.Last();
    } 
}