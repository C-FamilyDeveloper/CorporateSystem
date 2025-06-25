using System.Threading.Channels;
using Confluent.Kafka;
using CorporateSystem.SharedDocs.Kafka.Implementations;
using CorporateSystem.SharedDocs.Kafka.Interfaces;
using CorporateSystem.SharedDocs.Kafka.Models;
using CorporateSystem.SharedDocs.Kafka.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CorporateSystem.SharedDocs.Kafka;

public class KafkaAsyncConsumer<TKey, TEvent> : IDisposable
    where TEvent : IKeyedEvent<TKey>
{
    private static int ChannelCapacity => 10;
    private readonly IConsumer<TKey, TEvent> _consumer;
    private readonly IConsumerHandler<TKey, TEvent> _handler;
    private readonly Channel<ConsumeResult<TKey, TEvent>> _channel;
    private readonly ILogger<KafkaAsyncConsumer<TKey, TEvent>> _logger;

    public KafkaAsyncConsumer(
        IOptionsSnapshot<ConsumerOptions> options,
        [FromKeyedServices(nameof(UserConsumerHandler))] IConsumerHandler<TKey, TEvent> handler,
        ILogger<KafkaAsyncConsumer<TKey, TEvent>> logger,
        IDeserializer<TKey>? keyDeserializer = null,
        IDeserializer<TEvent>? valueDeserializer = null)
    {
        var optionsSnapshot = options.Get(typeof(TEvent).Name);
        _handler = handler;
        _logger = logger;
        logger.LogInformation($"{nameof(KafkaAsyncConsumer<TKey, TEvent>)}: consumer options: " +
                              $"bootstrap_server={optionsSnapshot.BootstrapServer}, " +
                              $"group_id={optionsSnapshot.GroupId}, " +
                              $"topics={string.Join(",", optionsSnapshot.Topics)}");
        var consumerBuilder = new ConsumerBuilder<TKey, TEvent>(new ConsumerConfig
        {
            BootstrapServers = optionsSnapshot.BootstrapServer,
            GroupId = optionsSnapshot.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            EnableAutoOffsetStore = false,
            SessionTimeoutMs = 10000,
            HeartbeatIntervalMs = 3000
        });

        if (keyDeserializer is not null)
        {
            consumerBuilder.SetKeyDeserializer(keyDeserializer);
        }

        if (valueDeserializer is not null)
        {
            consumerBuilder.SetValueDeserializer(valueDeserializer);
        }

        _consumer = consumerBuilder.Build();
        
        _consumer.Subscribe(optionsSnapshot.Topics);
        _channel = Channel.CreateBounded<ConsumeResult<TKey, TEvent>>(
            new BoundedChannelOptions(ChannelCapacity)
            {
                SingleWriter = true,
                SingleReader = true,
                AllowSynchronousContinuations = false,
                FullMode = BoundedChannelFullMode.Wait
            });
    }

    public async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var handleTask = HandleCoreAsync(cancellationToken);
        var consumeTask = ConsumeCoreAsync(cancellationToken);
        
        await Task.WhenAll(handleTask, consumeTask);
    }

    private async Task HandleCoreAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        
        await foreach (var consumeResults in _channel.Reader
                           .ReadAllAsync(cancellationToken))
        {
            try
            {
                await _handler.Handle([consumeResults], cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(HandleCoreAsync)}: {e.Message}");
                // todo: policy retry
            }

            _consumer.StoreOffset(consumeResults);
        }
    }

    private async Task ConsumeCoreAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        
        while (_consumer.Consume(cancellationToken) is { } consumeResult)
        {
            _logger.LogInformation($"{consumeResult.Partition.Value}:{consumeResult.Offset.Value}:WriteToChannel");
            await _channel.Writer.WriteAsync(consumeResult, cancellationToken);
        }
        
        _logger.LogInformation($"{nameof(ConsumeCoreAsync)}: Channel was closed");
        _channel.Writer.Complete();
    }

    public void Dispose()
    {
        _consumer.Dispose();
    }
}