using System.Text.Json;
using Confluent.Kafka;
using CorporateSystem.Auth.Kafka.Interfaces;
using CorporateSystem.Auth.Kafka.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CorporateSystem.Auth.Kafka.Implementations;

public class ProducerHandler<TKey, TEvent> : IProducerHandler<TEvent> 
    where TEvent : IKeyedEvent<TKey>
{
    protected readonly IProducer<TKey, string> _producer;
    protected readonly ProducerOptions _options;
    protected readonly ILogger<TEvent> _logger;

    public ProducerHandler(IOptions<ProducerOptions> options, ILogger<TEvent> logger)
    {
        _options = options.Value;
        _logger = logger;

        var fiveMilliseconds = 5.0;
        var sixteenKilobytes = 16324;
        
        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServer,
            LingerMs = fiveMilliseconds,
            BatchSize = sixteenKilobytes
        };
        
        _producer = new ProducerBuilder<TKey, string>(config).Build();
    }
    
    public virtual async Task ProduceAsync(IReadOnlyList<TEvent> data, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var @event in data)
            {
                await _producer.ProduceAsync(_options.Topic, new Message<TKey, string>
                {  
                    Key = @event.Key,
                    Value = JsonSerializer.Serialize(@event)
                }, cancellationToken);   
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