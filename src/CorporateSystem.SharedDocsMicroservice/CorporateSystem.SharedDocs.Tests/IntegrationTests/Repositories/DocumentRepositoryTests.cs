using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Migrations;
using CorporateSystem.SharedDocs.Infrastructure.Options;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Implementations;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;

namespace CorporateSystem.SharedDocs.Tests.IntegrationTests;

public class DocumentRepositoryTests : IClassFixture<PostgresContainer>
{
    private readonly PostgresContainer _fixture;

    public DocumentRepositoryTests(PostgresContainer postgresContainer)
    {
        _fixture = postgresContainer;
        
        var migrator = new Migrator(_fixture.ConnectionString);
        migrator.ApplyMigrations();
    }

    [Fact]
    public async Task GetAsync_ReturnsDocument_WhenIdExists()
    {
        // Arrange
        var repository = GetRepository();
        
        var createDocumentDto = new CreateDocumentDto
        {
            OwnerId = 1,
            Title = "Test Document",
            Content = "Test Content"
        };
        
        var createdIds = await repository.CreateAsync([createDocumentDto]);
        var documentId = createdIds[0];

        // Act
        var result = await repository.GetAsync(documentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Document", result.Title);
        Assert.Equal("Test Content", result.Content);
    }

    [Fact]
    public async Task CreateAsync_AddsDocumentsToDatabase()
    {
        // Arrange
        var repository = GetRepository();

        var createDocumentDto1 = new CreateDocumentDto
        {
            OwnerId = 1,
            Title = "New Document 1",
            Content = "New Content 1"
        };
        
        var createDocumentDto2 = new CreateDocumentDto
        {
            OwnerId = 2,
            Title = "New Document 2",
            Content = "New Content 2"
        };

        // Act
        var ids = await repository.CreateAsync([createDocumentDto1, createDocumentDto2]);

        // Assert
        Assert.Equal(2, ids.Length);

        var document1 = await repository.GetAsync(ids[0]);
        Assert.NotNull(document1);
        Assert.Equal("New Document 1", document1.Title);
        Assert.Equal("New Content 1", document1.Content);

        var document2 = await repository.GetAsync(ids[1]);
        Assert.NotNull(document2);
        Assert.Equal("New Document 2", document2.Title);
        Assert.Equal("New Content 2", document2.Content);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesDocumentInDatabase()
    {
        // Arrange
        var repository = GetRepository();
        
        var createDocumentDto = new CreateDocumentDto
        {
            OwnerId = 1,
            Title = "Old Title",
            Content = "Old Content"
        };
        
        var createdIds = await repository.CreateAsync([createDocumentDto]);
        var documentId = createdIds[0];

        var updateDocumentDto = new UpdateDocumentDto
        {
            OwnerId = 1,
            Title = "Updated Title",
            Content = "Updated Content"
        };

        // Act
        await repository.UpdateAsync(documentId, updateDocumentDto);

        // Assert
        var updatedDocument = await repository.GetAsync(documentId);
        Assert.NotNull(updatedDocument);
        Assert.Equal("Updated Title", updatedDocument.Title);
        Assert.Equal("Updated Content", updatedDocument.Content);
    }

    [Fact]
    public async Task DeleteAsync_RemovesDocumentsFromDatabase()
    {
        // Arrange
        var repository = GetRepository();
        
        var createDocumentDto = new CreateDocumentDto
        {
            OwnerId = 1,
            Title = "Test Document",
            Content = "Test Content"
        };
        var createdIds = await repository.CreateAsync([createDocumentDto]);
        var documentId = createdIds[0];

        // Act
        await repository.DeleteAsync([documentId]);

        // Assert
        var deletedDocument = await repository.GetAsync(documentId);
        Assert.Null(deletedDocument);
    }
    
    private IDocumentRepository GetRepository() => 
        new DocumentRepository(Options.Create(new PostgresOptions
        {
            ConnectionString = _fixture.ConnectionString
        }));
}