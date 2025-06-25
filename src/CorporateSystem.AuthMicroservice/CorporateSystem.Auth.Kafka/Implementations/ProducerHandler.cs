using Confluent.Kafka;
using CorporateSystem.Auth.Kafka.Interfaces;
using CorporateSystem.Auth.Kafka.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CorporateSystem.Auth.Kafka.Implementations;

internal class ProducerHandler<TKey, TEvent> : IProducerHandler<TKey, TEvent> 
    where TEvent : IKeyedEvent<TKey>
{
    private readonly IProducer<TKey, TEvent> _producer;
    private readonly ProducerOptions _options;
    private readonly ILogger<TEvent> _logger;

    public ProducerHandler(
        IOptionsSnapshot<ProducerOptions> options,
        ILogger<TEvent> logger,
        ISerializer<TKey>? keySerializer,
        ISerializer<TEvent>? valueSerializer)
    {
        _options = options.Get(typeof(TEvent).Name);
        _logger = logger;

        var fiveMilliseconds = 5.0;
        var sixteenKilobytes = 16324;
        
        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServer,
            LingerMs = fiveMilliseconds,
            BatchSize = sixteenKilobytes
        };
        
        var producerBuilder = new ProducerBuilder<TKey, TEvent>(config);

        if (keySerializer is not null)
        {
            producerBuilder.SetKeySerializer(keySerializer);
        }

        if (valueSerializer is not null)
        {
            producerBuilder.SetValueSerializer(valueSerializer);
        }
        
        _producer = producerBuilder.Build();
    }
    
    public async Task ProduceAsync(IReadOnlyList<TEvent> data, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var @event in data)
            {
                _logger.LogInformation($"{nameof(ProducerHandler<TKey, TEvent>)}: attempt to produce {@event.ToString()} in topic {_options.Topic}");
                await _producer.ProduceAsync(_options.Topic, new Message<TKey, TEvent>
                {  
                    Key = @event.Key,
                    Value = @event
                }, cancellationToken);   
                
                _logger.LogInformation($"{nameof(ProducerHandler<TKey, TEvent>)}: successfully to produce {@event.ToString()} in topic {_options.Topic}");
            }
        }
        catch (ProduceException<Ignore, string> ex)
        {
            _logger.LogError($"{nameof(ProducerHandler<TKey, TEvent>)}: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}