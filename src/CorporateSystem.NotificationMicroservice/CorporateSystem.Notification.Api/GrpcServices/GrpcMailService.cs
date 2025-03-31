using Api;
using CorporateSystem.Services.Dtos;
using CorporateSystem.Services.Services.Interfaces;
using Grpc.Core;

namespace CorporateSystem.Notification.Api.GrpcServices;

public class GrpcMailService(IEmailSenderService emailSenderService) : MailService.MailServiceBase
{
    public override async Task<SendMessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request);

            await emailSenderService
                .SendMailAsync(new EmailSendDto(
                    request.Token,
                    request.Title,
                    request.Message,
                    request.ReceiverEmails.ToArray()));

            return new SendMessageResponse();
        }
        catch (Exception e)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"Internal server error: {e.Message}"));
        }
    }
}