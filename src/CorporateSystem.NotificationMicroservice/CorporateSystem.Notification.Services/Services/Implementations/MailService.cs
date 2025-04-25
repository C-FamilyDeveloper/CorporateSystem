using CorporateSystem.Services.Dtos;
using CorporateSystem.Services.Services.Interfaces;

namespace CorporateSystem.Services.Services.Implementations;

internal class MailService : IMailService
{
    public Task SendMailAsync(SendMailDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}