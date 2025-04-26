using CorporateSystem.Services.Dtos;
using CorporateSystem.Services.Options;
using CorporateSystem.Services.Services.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CorporateSystem.Services.Services.Implementations;

internal class MailService(IOptions<EmailOptions> emailOptions) : IMailService
{
    public async Task SendMailAsync(SendMailDto dto, CancellationToken cancellationToken = default)
    {
        var emailOptionsSnapshot = emailOptions.Value;
        
        using var emailMessage = new MimeMessage
        {
            Subject = dto.Title,
            Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = dto.Message
            }
        };
 
        emailMessage.From.Add(new MailboxAddress("", emailOptionsSnapshot.Login));
        
        foreach (var receiverEmail in dto.ReceiverEmails)
        {
            emailMessage.To.Add(new MailboxAddress("", receiverEmail));
        }

        using var client = new SmtpClient();
        await client.ConnectAsync(emailOptionsSnapshot.Host, emailOptionsSnapshot.Port, false, cancellationToken);
        await client.AuthenticateAsync(emailOptionsSnapshot.Login, emailOptionsSnapshot.Password, cancellationToken);
        await client.SendAsync(emailMessage, cancellationToken);
 
        await client.DisconnectAsync(true, cancellationToken);
    }
}