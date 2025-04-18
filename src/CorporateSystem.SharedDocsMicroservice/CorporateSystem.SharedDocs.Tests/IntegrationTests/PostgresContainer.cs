using CorporateSystem.SharedDocs.Infrastructure.Migrations;
using Testcontainers.PostgreSql;

namespace CorporateSystem.SharedDocs.Tests.IntegrationTests;

public class PostgresContainer : IAsyncLifetime
{
    private PostgreSqlContainer  _postgresContainer;
    public string ConnectionString { get; set; }
    
    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithPortBinding(5432, true)
            .Build();
        
        await _postgresContainer.StartAsync();
        ConnectionString = _postgresContainer.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.StopAsync();
    }
}