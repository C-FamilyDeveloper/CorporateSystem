namespace CorporateSystem.Auth.Kafka.Options;

public class ProducerOptions
{
    public required string BootstrapServer { get; init; }
    public required string Topic { get; init; }
}