using CorporateSystem.Services.Dtos;
using CorporateSystem.Services.Options;
using CorporateSystem.Services.Services.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CorporateSystem.Services.Services.Implementations;

internal sealed class MailService(IOptions<EmailOptions> emailOptions) : IMailService
{
    public async Task SendMailAsync(SendMailDto sendMailDto, CancellationToken cancellationToken = default)
    {
        var emailOptionsSnapshot = emailOptions.Value;
        
        using var emailMessage = new MimeMessage
        {
            Subject = sendMailDto.Title,
            Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = sendMailDto.Message
            }
        };
 
        emailMessage.From.Add(new MailboxAddress("", emailOptionsSnapshot.Login));
        
        foreach (var receiverEmail in sendMailDto.ReceiverEmails)
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