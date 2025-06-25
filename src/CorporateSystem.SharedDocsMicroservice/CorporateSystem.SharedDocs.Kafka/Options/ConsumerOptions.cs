namespace CorporateSystem.SharedDocs.Kafka.Options;

public class ConsumerOptions
{
    public required string BootstrapServer { get; init; }
    public required string GroupId { get; init; }
    public required string[] Topics { get; init; }
}