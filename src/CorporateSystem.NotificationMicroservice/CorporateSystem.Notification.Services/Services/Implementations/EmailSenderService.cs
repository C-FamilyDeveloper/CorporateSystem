using CorporateSystem.Services.Dtos;
using CorporateSystem.Services.Extensions;
using CorporateSystem.Services.Services.Interfaces;

namespace CorporateSystem.Services.Services.Implementations;

internal class EmailSenderService(
    IMailService mailService,
    IFakeMailService fakeMailService) 
    : IEmailSenderService
{
    public async Task SendMailAsync(
        EmailSendDto dto,
        CancellationToken cancellationToken = default)
    {
        dto.MustBeValid();

        var fakeEmails = new List<string>();
        var emails = new List<string>();
        
        foreach (var receiverEmail in dto.ReceiverEmails)
        {
            if (receiverEmail.EndsWith("@bobr.ru"))
            {
                fakeEmails.Add(receiverEmail);
            }
            else
            {
                emails.Add(receiverEmail);
            }
        }
        
        var tasks = new List<Task>
        {
            mailService.SendMailAsync(new SendMailDto(dto.Token, dto.Title, dto.Message, emails.ToArray()), cancellationToken),
            fakeMailService.SendMailAsync(new SendMailDto(dto.Token, dto.Title, dto.Message, fakeEmails.ToArray()), cancellationToken)
        };

        await Task.WhenAll(tasks);
    }
}