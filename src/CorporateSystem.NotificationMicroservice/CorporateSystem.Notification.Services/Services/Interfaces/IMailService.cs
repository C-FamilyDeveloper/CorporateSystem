using CorporateSystem.Services.Dtos;

namespace CorporateSystem.Services.Services.Interfaces;

public interface IMailService
{
    Task SendMailAsync(SendMailDto dto, CancellationToken cancellationToken = default);
}