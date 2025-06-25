using System.Runtime.Serialization;
using System.Text.Json;
using Confluent.Kafka;
using CorporateSystem.SharedDocs.Kafka.Interfaces;
using CorporateSystem.SharedDocs.Kafka.Models;
using CorporateSystem.SharedDocs.Kafka.Options;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CorporateSystem.SharedDocs.Kafka.Implementations;

internal class UserConsumerHandler(
    ILogger<UserConsumerHandler> logger,
    IDocumentService documentService)
    : IConsumerHandler<Null, UserDeleteEvent>
{
    public async Task Handle(
        IReadOnlyList<ConsumeResult<Null, UserDeleteEvent>> messages,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIds = messages.Select(result => result.Message.Value.UserId).ToArray();
            
            logger.LogInformation($"{nameof(UserConsumerHandler)}: Received userIds={string.Join(",", userIds)}");
            
            var deleteDocumentOwnerTask = documentService.DeleteDocumentAsync(new DeleteDocumentDto
            {
                OwnerIds = userIds
            }, cancellationToken);

            var deleteDocumentUsersTask = documentService.DeleteDocumentUsersAsync(new DeleteDocumentUsersDto
            {
                UserIds = userIds
            }, cancellationToken);
            
            await Task.WhenAll(deleteDocumentOwnerTask, deleteDocumentUsersTask);
        }
        catch (Exception ex)
        {
            logger.LogError($"{nameof(UserConsumerHandler)}: {ex.Message}");
            throw;
        }  
    }
}