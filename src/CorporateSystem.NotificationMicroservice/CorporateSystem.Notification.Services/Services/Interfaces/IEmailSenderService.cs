using CorporateSystem.Services.Dtos;

namespace CorporateSystem.Services.Services.Interfaces;

public interface IEmailSenderService
{
    Task SendMailAsync(EmailSendDto emailSendDto, CancellationToken cancellationToken = default);
}