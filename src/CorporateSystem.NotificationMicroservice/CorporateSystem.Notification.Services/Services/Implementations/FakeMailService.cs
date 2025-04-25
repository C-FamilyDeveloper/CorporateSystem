using System.Transactions;
using CorporateSystem.Infrastructure.Repositories.Interfaces;
using CorporateSystem.Notification.Domain.Entities;
using CorporateSystem.Services.Dtos;
using CorporateSystem.Services.Extensions;
using CorporateSystem.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CorporateSystem.Services.Services.Implementations;

public interface IFakeMailService : IMailService;
internal class FakeMailService(
    IFakeMailApiService fakeMailApiService,
    IContextFactory contextFactory,
    ILogger<IFakeMailService> logger) : IFakeMailService
{
    public async Task SendMailAsync(SendMailDto dto, CancellationToken cancellationToken = default)
    {
        dto.MustBeValid(logger);
        
        var request = new SendEmailMessageRequest
        {
            Message = dto.Message,
            Title = dto.Title,
            ReceiverEmails = dto.ReceiverEmails,
            Token = dto.Token
        };
        
        logger.LogInformation($"{nameof(SendMailAsync)}: Call method: SendEmailMessageAsync");
        
        await fakeMailApiService.SendEmailMessageAsync(request, cancellationToken);

        logger.LogInformation($"{nameof(SendMailAsync)}: Call method: GetEmailByTokenAsync");
        var senderEmail = (await fakeMailApiService.GetEmailByTokenAsync(new GetEmailByTokenRequest
        {
            Token = dto.Token
        }, cancellationToken)).Email;
        
        logger.LogInformation($"{nameof(SendMailAsync)}: Writing email message to db");
        await contextFactory.ExecuteWithCommitAsync(async context =>
        {
            foreach (var receiverEmail in dto.ReceiverEmails)
            {
                await context.EmailMessages.AddAsync(new EmailMessage
                {
                    Message = dto.Message,
                    ReceiverEmail = receiverEmail,
                    SenderEmail = senderEmail,
                    CreatedAtUtc = DateTime.UtcNow
                }, cancellationToken);   
            }
        }, IsolationLevel.ReadUncommitted, cancellationToken);
        
        logger.LogInformation($"{nameof(SendMailAsync)}: Writing completed successfully");
    }
}