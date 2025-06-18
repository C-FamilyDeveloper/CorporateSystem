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

internal class UserConsumerHandler : IConsumerHandler<Ignore, string>
{
    private readonly ILogger<UserConsumerHandler> _logger;
    private readonly IDocumentService _documentService;
    
    public UserConsumerHandler(
        ILogger<UserConsumerHandler> logger, 
        IDocumentService documentService)
    {
        _logger = logger;
        _documentService = documentService;
    }
    
    public async Task Handle(
        IReadOnlyList<ConsumeResult<Ignore, string>> messages,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userDeleteEvents = messages
                .Select(message =>
                    JsonSerializer.Deserialize<UserDeleteEvent>(message.Message.Value)
                    ?? throw new ArgumentNullException(message: message.Message.Value,
                        innerException: null))
                .ToList();

            var userIds = userDeleteEvents.Select(@event => @event.UserId).ToArray();
            
            var deleteDocumentOwnerTask = _documentService.DeleteDocumentAsync(new DeleteDocumentDto
            {
                OwnerIds = userIds
            }, cancellationToken);

            var deleteDocumentUsersTask = _documentService.DeleteDocumentUsersAsync(new DeleteDocumentUsersDto
            {
                UserIds = userIds
            }, cancellationToken);
            
            await Task.WhenAll(deleteDocumentOwnerTask, deleteDocumentUsersTask);
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(UserConsumerHandler)}: {ex.Message}");
            throw;
        }  
    }
}