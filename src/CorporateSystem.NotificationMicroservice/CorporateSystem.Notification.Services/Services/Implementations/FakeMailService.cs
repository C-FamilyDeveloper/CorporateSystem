using System.Transactions;
using CorporateSystem.Infrastructure.Repositories;
using CorporateSystem.Infrastructure.Repositories.Interfaces;
using CorporateSystem.Notification.Domain.Entities;
using CorporateSystem.Services.Dtos;
using CorporateSystem.Services.Extensions;
using CorporateSystem.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CorporateSystem.Services.Services.Implementations;

public interface IFakeMailService : IMailService;

internal sealed class FakeMailService(
    IFakeMailApiService fakeMailApiService,
    IContextFactory<DataContext> contextFactory,
    ILogger<IFakeMailService> logger) : IFakeMailService
{
    public async Task SendMailAsync(SendMailDto sendMailDto, CancellationToken cancellationToken = default)
    {
        sendMailDto.MustBeValid(logger);
        
        var request = new SendEmailMessageRequest
        {
            Message = sendMailDto.Message,
            Title = sendMailDto.Title,
            ReceiverEmails = sendMailDto.ReceiverEmails,
            Token = sendMailDto.Token
        };
        
        logger.LogInformation($"{nameof(SendMailAsync)}: Call method: SendEmailMessageAsync");
        
        await fakeMailApiService.SendEmailMessageAsync(request, cancellationToken);

        logger.LogInformation($"{nameof(SendMailAsync)}: Call method: GetEmailByTokenAsync");
        var senderEmail = (await fakeMailApiService.GetEmailByTokenAsync(new GetEmailByTokenRequest
        {
            Token = sendMailDto.Token
        }, cancellationToken)).Email;
        
        logger.LogInformation($"{nameof(SendMailAsync)}: Writing email message to db");
        await contextFactory.ExecuteWithCommitAsync(async context =>
        {
            await context.EmailMessages.AddRangeAsync(
                sendMailDto.ReceiverEmails.Select(receiverEmail => new EmailMessage
                {
                    Message = sendMailDto.Message,
                    ReceiverEmail = receiverEmail,
                    SenderEmail = senderEmail,
                    CreatedAtUtc = DateTime.UtcNow
                }), cancellationToken);
        }, IsolationLevel.ReadUncommitted, cancellationToken);
        
        logger.LogInformation($"{nameof(SendMailAsync)}: Writing completed successfully");
    }
}