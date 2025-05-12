using CorporateSystem.SharedDocs.Infrastructure.Migrations;
using CorporateSystem.SharedDocs.Infrastructure.Options;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Implementations;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace CorporateSystem.SharedDocs.Tests.IntegrationTests.Repositories;

public class BaseRepositoryTests : IClassFixture<PostgresContainer>
{
    protected readonly PostgresContainer Fixture;

    public BaseRepositoryTests(PostgresContainer postgresContainer)
    {
        Fixture = postgresContainer;
        
        var migrator = new Migrator(Fixture.ConnectionString);
        migrator.ApplyMigrations();
    }
    
    protected IDocumentUserRepository GetDocumentUserRepository() =>
        new DocumentUserRepository(Options.Create(new PostgresOptions
        {
            ConnectionString = Fixture.ConnectionString
        }));

    protected IDocumentCompositeRepository GetDocumentCompositeRepository() =>
        new DocumentCompositeRepository(Options.Create(new PostgresOptions
        {
            ConnectionString = Fixture.ConnectionString
        }));
    
    protected IDocumentRepository GetDocumentRepository() =>
        new DocumentRepository(Options.Create(new PostgresOptions
        {
            ConnectionString = Fixture.ConnectionString
        }));
}