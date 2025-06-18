using System.Threading.Channels;
using Confluent.Kafka;
using CorporateSystem.SharedDocs.Kafka.Extensions;
using CorporateSystem.SharedDocs.Kafka.Interfaces;
using Microsoft.Extensions.Logging;

namespace CorporateSystem.SharedDocs.Kafka;

public class KafkaAsyncConsumer<TKey, TValue>
{
    private static int ChannelCapacity => 10;
    private readonly IConsumer<TKey, TValue> _consumer;
    private readonly IConsumerHandler<TKey, TValue> _handler;
    private readonly Channel<ConsumeResult<TKey, TValue>> _channel;
    private readonly ILogger<KafkaAsyncConsumer<TKey, TValue>> _logger;

    public KafkaAsyncConsumer(
        string bootstrapServers,
        string groupId,
        string[] topics,
        IConsumerHandler<TKey, TValue> handler,
        ILogger<KafkaAsyncConsumer<TKey, TValue>> logger)
    {
        _handler = handler;
        _logger = logger;
        _consumer = new ConsumerBuilder<TKey, TValue>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            EnableAutoOffsetStore = false
        }).Build();
        
        _consumer.Subscribe(topics);
        _channel = Channel.CreateBounded<ConsumeResult<TKey, TValue>>(
            new BoundedChannelOptions(ChannelCapacity)
            {
                SingleWriter = true,
                SingleReader = true,
                AllowSynchronousContinuations = true,
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
                           .ReadAllAsync(cancellationToken)
                           .Buffer(ChannelCapacity)
                           .WithCancellation(cancellationToken))
        {
            try
            {
                await _handler.Handle(consumeResults, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(HandleCoreAsync)}: {e.Message}");
                // todo: policy retry
            }
            
            var partitionLastOffsets = consumeResults
                .GroupBy(
                    consumeResult => consumeResult.Partition.Value,
                    (_, consumeResultsList) => 
                        consumeResultsList.MaxBy(p => p.Offset.Value))
                .ToArray();

            foreach (var partitionLastOffset in partitionLastOffsets)
            {
                _consumer.StoreOffset(partitionLastOffset);
            }
        }
    }

    private async Task ConsumeCoreAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        while (_consumer.Consume(cancellationToken) is { } consumeResult)
        {
            await _channel.Writer.WriteAsync(consumeResult, cancellationToken);
            _logger.LogTrace($"{consumeResult.Partition.Value}:{consumeResult.Offset.Value}:WriteToChannel");
        }
        
        _channel.Writer.Complete();
    }
}