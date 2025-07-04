using CorporateSystem.Services.Dtos;
using CorporateSystem.Services.Extensions;
using CorporateSystem.Services.Services.Factory;
using CorporateSystem.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CorporateSystem.Services.Services.Implementations;

internal sealed class EmailSenderService(
    IEmailServiceFactory emailServiceFactory,
    ILogger<EmailSenderService> logger) 
    : IEmailSenderService
{
    public async Task SendMailAsync(
        EmailSendDto emailSendDto,
        CancellationToken cancellationToken = default)
    {
        emailSendDto.MustBeValid(logger);

        var groupedServices = emailSendDto.ReceiverEmails
            .GroupBy(emailServiceFactory.Build)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        var tasks = new List<Task>();

        foreach (var (service, receiverEmails) in groupedServices)
        {
            tasks.Add(service
                .SendMailAsync(new SendMailDto(
                    emailSendDto.Token,
                    emailSendDto.Title, 
                    emailSendDto.Message,
                    receiverEmails.ToArray()),
                cancellationToken));
        }

        await Task.WhenAll(tasks);
    }
}