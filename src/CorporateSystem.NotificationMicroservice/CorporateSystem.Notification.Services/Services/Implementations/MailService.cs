using CorporateSystem.Services.Dtos;
using CorporateSystem.Services.Services.Interfaces;

namespace CorporateSystem.Services.Services.Implementations;

internal class MailService : IMailService
{
    public Task SendMailAsync(SendMailDto dto, CancellationToken cancellationToken = default)
    {
        // Заглушка, чтобы не натыкаться на исключение в тестах
        if (!dto.ReceiverEmails.Any())
            return Task.CompletedTask;
        
        throw new NotImplementedException();
    }
}