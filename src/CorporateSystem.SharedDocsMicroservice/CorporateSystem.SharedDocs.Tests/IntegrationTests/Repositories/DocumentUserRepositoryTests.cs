using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Migrations;
using CorporateSystem.SharedDocs.Infrastructure.Options;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Implementations;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using CorporateSystem.SharedDocs.Tests.Helpers;
using Microsoft.Extensions.Options;

namespace CorporateSystem.SharedDocs.Tests.IntegrationTests.Repositories;

[Collection("PostgresCollection")]
public class DocumentUserRepositoryTests : IClassFixture<PostgresContainer>
{
    private readonly PostgresContainer _fixture;

    public DocumentUserRepositoryTests(PostgresContainer postgresContainer)
    {
        _fixture = postgresContainer;
        
        var migrator = new Migrator(_fixture.ConnectionString);
        migrator.ApplyMigrations();
    }
    
    [Fact]
    public async Task GetAsync_WithExistingId_ReturnsDocumentUser()
    {
        // Arrange
        var repository = GetRepository();

        var documentId = Int.GetUniqueNumber();
        var userId = Int.GetUniqueNumber();
        
        var createDto = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = userId,
            AccessLevel = AccessLevel.Reader
        };

        var createdIds = await repository.CreateAsync([createDto]);
        var documentUserId = createdIds[0];

        // Act
        var result = await repository.GetAsync(documentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(documentUserId, result.Id);
        Assert.Equal(AccessLevel.Reader, result.AccessLevel);
        Assert.Equal(documentId, result.DocumentId);
        Assert.Equal(userId, result.UserId);
    }
    
    [Fact]
    public async Task GetAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var repository = GetRepository();

        var id = Int.GetUniqueNumber();
        
        // Act
        var result = await repository.GetAsync(id);

        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetAsync_WithIdsFilter_ReturnsFilteredDocumentUsers()
    {
        // Arrange
        var repository = GetRepository();
        
        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Writer
        };

        var createdIds = await repository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            Ids = [createdIds[0]]
        };

        // Act
        var result = (await repository.GetAsync(filter)).ToArray();

        // Assert
        Assert.Single(result);
        var documentUser = result.First();
        Assert.Equal(createdIds[0], documentUser.Id);
        Assert.Equal(AccessLevel.Reader, documentUser.AccessLevel);
    }
    
    [Fact]
    public async Task GetAsync_WithAccessLevelsFilter_ReturnsFilteredDocumentUsers()
    {
        // Arrange
        var repository = GetRepository();

        var documentId = Int.GetUniqueNumber();
        var userId = Int.GetUniqueNumber();
        
        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = userId,
            AccessLevel = AccessLevel.Writer
        };

        await repository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            AccessLevels = [AccessLevel.Writer]
        };

        // Act
        var result = (await repository.GetAsync(filter)).ToArray();

        // Assert
        // Другие тесты могут добавить свои записи в таблицу с access_level = writer,
        // поэтому ищем ту, которая создана в этом тесте
        Assert.NotEmpty(result);
        Assert.Contains(result
            .Where(documentUser => documentUser.AccessLevel is AccessLevel.Writer),
            documentUser => documentUser.DocumentId == documentId && documentUser.UserId == userId);
    }
    
    [Fact]
    public async Task GetAsync_WithUserIdsFilter_ReturnsFilteredDocumentUsers()
    {
        // Arrange
        var repository = GetRepository();

        var documentId = Int.GetUniqueNumber();
        var userId = Int.GetUniqueNumber();
        
        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = userId,
            AccessLevel = AccessLevel.Writer
        };

        await repository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            UserIds = [userId]
        };

        // Act
        var result = (await repository.GetAsync(filter)).ToArray();

        // Assert
        Assert.Single(result);
        var documentUser = result.First();
        Assert.Equal(userId, documentUser.UserId);
    }
    
    [Fact]
    public async Task CreateAsync_AddsSingleDocumentUserToDatabase()
    {
        // Arrange
        var repository = GetRepository();

        var documentId = Int.GetUniqueNumber();
        var userId = Int.GetUniqueNumber();
        
        var createDto = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = userId,
            AccessLevel = AccessLevel.Reader
        };

        // Act
        var ids = await repository.CreateAsync([createDto]);

        // Assert
        Assert.Single(ids);
        var documentUser = await repository.GetAsync(ids[0]);
        Assert.NotNull(documentUser);
        Assert.Equal(AccessLevel.Reader, documentUser.AccessLevel);
        Assert.Equal(documentId, documentUser.DocumentId);
        Assert.Equal(userId, documentUser.UserId);
    }
    
    [Fact]
    public async Task GetAsync_WithDocumentIdsFilter_ReturnsFilteredDocumentUsers()
    {
        // Arrange
        var repository = GetRepository();

        var documentId = Int.GetUniqueNumber();
        
        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Writer
        };

        await repository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            DocumentIds = [documentId]
        };

        // Act
        var result = (await repository.GetAsync(filter)).ToArray();

        // Assert
        Assert.Single(result);
        var documentUser = result.First();
        Assert.Equal(documentId, documentUser.DocumentId);
    }
    
    [Fact]
    public async Task CreateAsync_AddsMultipleDocumentUsersToDatabase()
    {
        // Arrange
        var repository = GetRepository();
        
        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Writer
        };

        // Act
        var ids = await repository.CreateAsync([createDto1, createDto2]);

        // Assert
        Assert.Equal(2, ids.Length);

        var documentUser1 = await repository.GetAsync(ids[0]);
        Assert.NotNull(documentUser1);
        Assert.Equal(AccessLevel.Reader, documentUser1.AccessLevel);

        var documentUser2 = await repository.GetAsync(ids[1]);
        Assert.NotNull(documentUser2);
        Assert.Equal(AccessLevel.Writer, documentUser2.AccessLevel);
    }
    
    [Fact]
    public async Task UpdateAsync_UpdatesDocumentUserInDatabase()
    {
        // Arrange
        var repository = GetRepository();

        var createDto = new CreateDocumentUserDto
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };

        var createdIds = await repository.CreateAsync([createDto]);
        var documentUserId = createdIds[0];

        var newDocumentId = Int.GetUniqueNumber();
        var newUserId = Int.GetUniqueNumber();
        
        var updateDto = new UpdateDocumentUserDto
        {
            DocumentId = newDocumentId,
            UserId = newUserId,
            AccessLevel = AccessLevel.Writer
        };

        // Act
        await repository.UpdateAsync(documentUserId, updateDto);

        // Assert
        var updatedDocumentUser = await repository.GetAsync(documentUserId);
        Assert.NotNull(updatedDocumentUser);
        Assert.Equal(AccessLevel.Writer, updatedDocumentUser.AccessLevel);
        Assert.Equal(newDocumentId, updatedDocumentUser.DocumentId);
        Assert.Equal(newUserId, updatedDocumentUser.UserId);
    }
    
    [Fact]
    public async Task DeleteAsync_RemovesDocumentUsersFromDatabase()
    {
        // Arrange
        var repository = GetRepository();

        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Writer
        };

        var createdIds = await repository.CreateAsync([createDto1, createDto2]);

        // Act
        await repository.DeleteAsync(createdIds);

        // Assert
        var documentUser1 = await repository.GetAsync(createdIds[0]);
        Assert.Null(documentUser1);

        var documentUser2 = await repository.GetAsync(createdIds[1]);
        Assert.Null(documentUser2);
    }
    
    private IDocumentUserRepository GetRepository() =>
        new DocumentUserRepository(Options.Create(new PostgresOptions
        {
            ConnectionString = _fixture.ConnectionString
        }));
}