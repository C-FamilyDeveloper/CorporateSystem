using System.Text.Json;
using CorporateSystem.Auth.Infrastructure;
using CorporateSystem.Auth.Infrastructure.Repositories.Interfaces;
using CorporateSystem.Auth.Kafka.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CorporateSystem.Auth.Api.Background.Services;

public class OutboxEventBackgroundService(
    IEventHandlerFactory eventHandlerFactory,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<OutboxEventBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken); // чтобы миграции точно выполнились
        logger.LogInformation($"{nameof(OutboxEventBackgroundService)}: I woke up!");
        while (!cancellationToken.IsCancellationRequested)
        {
            using var scope = serviceScopeFactory.CreateScope();

            var contextFactory = scope.ServiceProvider.GetRequiredService<IContextFactory<DataContext>>();
            
            await contextFactory.ExecuteWithCommitAsync(async context =>
            {
                var outboxEvents = await context.OutboxEvents
                    .Where(e => !e.Processed)
                    .Take(100)
                    .ToListAsync(cancellationToken);

                foreach (var outboxEvent in outboxEvents)
                {
                    try
                    {
                        var eventType = Type.GetType(outboxEvent.EventType);
                        if (eventType is null)
                        {
                            throw new InvalidOperationException($"{nameof(OutboxEventBackgroundService)}:Неизвестный тип события: {outboxEvent.EventType}");
                        }

                        var @event = JsonSerializer.Deserialize(outboxEvent.Payload, eventType);

                        if (@event is null)
                        {
                            throw new ArgumentNullException(
                                $"{nameof(OutboxEventBackgroundService)}: event_id={outboxEvent.Id}, payload={outboxEvent.Payload}");
                        }

                        var eventHandler = eventHandlerFactory.GetHandler(outboxEvent.EventType);
                        await eventHandler.HandleAsync([@event], cancellationToken);
                        
                        outboxEvent.Processed = true;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{nameof(OutboxEventBackgroundService)}: Ошибка при обработке события {outboxEvent.Id}: {ex.Message}");
                    }
                }
                
                await context.SaveChangesAsync(cancellationToken);
            }, cancellationToken: cancellationToken);
            
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
    }
}