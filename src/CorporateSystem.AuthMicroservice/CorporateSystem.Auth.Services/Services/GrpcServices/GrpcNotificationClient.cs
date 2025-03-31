using CorporateSystem.Auth.Services.Options;
using Grpc;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;

namespace CorporateSystem.Auth.Services.Services.GrpcServices;

public class GrpcNotificationClient(IOptions<GrpcNotificationOptions> grpcNotificationOptions)
{
    private readonly GrpcNotificationOptions _grpcNotificationOptions = grpcNotificationOptions.Value;

    public async Task SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        using var channel = GrpcChannel.ForAddress(_grpcNotificationOptions.ConnectionString);
        var client = new MailService.MailServiceClient(channel);
        await client.SendMessageAsync(request, cancellationToken: cancellationToken);
    }
}