namespace CorporateSystem.Services.Options;

public class EmailOptions
{
    public required string Host { get; init; }
    public int Port { get; init; }
    public required string Login { get; init; }
    public required string Password { get; init; }
}