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
        var repository = GetDocumentUserRepository();

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
        var repository = GetDocumentUserRepository();

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
        var repository = GetDocumentUserRepository();
        
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
        var repository = GetDocumentUserRepository();

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
        var repository = GetDocumentUserRepository();

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
    public async Task GetAsync_WithOwnerIdsFilter_ReturnsFilteredDocuments()
    {
        // Arrange
        var repository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var ownerId1 = Int.GetUniqueNumber();
        var ownerId2 = Int.GetUniqueNumber();

        var document1 = new CreateDocumentDto
        {
            OwnerId = ownerId1,
            Title = "Doc1",
            Content = "Content1"
        };
        var document2 = new CreateDocumentDto
        {
            OwnerId = ownerId2,
            Title = "Doc2",
            Content = "Content2"
        };
        
        var createdDocumentIds = await documentRepository.CreateAsync([document1, document2]);

        var documentUser1 = new CreateDocumentUserDto
        {
            DocumentId = createdDocumentIds[0],
            AccessLevel = AccessLevel.Writer,
            UserId = ownerId1
        };

        var documentUser2 = new CreateDocumentUserDto
        {
            DocumentId = createdDocumentIds[1],
            AccessLevel = AccessLevel.Writer,
            UserId = ownerId2
        };

        await repository.CreateAsync([documentUser1, documentUser2]);
        
        var filter = new DocumentInfoFilter
        {
            OwnerIds = [ownerId1]
        };

        // Act
        var result = (await repository.GetAsync(filter)).ToArray();

        // Assert
        Assert.Single(result);
        var documentInfo = result.First();
        Assert.Equal(createdDocumentIds[0], documentInfo.Id);
        Assert.Equal("Doc1", documentInfo.Title);
    }
    
    [Fact]
    public async Task GetAsync_WithFollowerIdsFilter_ReturnsFilteredDocuments()
    {
        // Arrange
        var repository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();
        
        var ownerId = Int.GetUniqueNumber();
        var followerId1 = Int.GetUniqueNumber();
        var followerId2 = Int.GetUniqueNumber();

        var document1 = new CreateDocumentDto
        {
            OwnerId = ownerId,
            Title = "Title1"
        };
        
        var document2 = new CreateDocumentDto
        {
            OwnerId = ownerId,
            Title = "Title2"
        };

        var documentIds = await documentRepository.CreateAsync([document1, document2]);
        
        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = documentIds[0],
            UserId = followerId1,
            AccessLevel = AccessLevel.Reader
        };
        
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = documentIds[1],
            UserId = followerId2,
            AccessLevel = AccessLevel.Writer
        };

        await repository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentInfoFilter
        {
            FollowerIds = [followerId1]
        };

        // Act
        var result = (await repository.GetAsync(filter)).ToArray();

        // Assert
        Assert.Single(result);
        var documentInfo = result.First();
        Assert.Equal(documentIds[0], documentInfo.Id);
    }
    
    [Fact]
    public async Task CreateAsync_AddsSingleDocumentUserToDatabase()
    {
        // Arrange
        var repository = GetDocumentUserRepository();

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
        var repository = GetDocumentUserRepository();

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
        var repository = GetDocumentUserRepository();
        
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
        var repository = GetDocumentUserRepository();

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
    public async Task DeleteAsync_WithIdsFilter_DeletesMatchingDocumentUsers()
    {
        // Arrange
        var repository = GetDocumentUserRepository();

        var documentId = Int.GetUniqueNumber();
        var userId = Int.GetUniqueNumber();

        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = userId,
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
        await repository.DeleteAsync(filter);

        // Assert
        var remainingDocumentUsers = (await repository.GetAsync((DocumentUserFilter)null)).ToArray();
        Assert.True(remainingDocumentUsers.All(documentUser => documentUser.Id != createdIds[0]));
        Assert.Contains(remainingDocumentUsers, documentUser => documentUser.Id == createdIds[1]);
    }
    
    [Fact]
    public async Task DeleteAsync_WithDocumentIdsFilter_DeletesMatchingDocumentUsers()
    {
        // Arrange
        var repository = GetDocumentUserRepository();

        var documentId1 = Int.GetUniqueNumber();
        var documentId2 = Int.GetUniqueNumber();

        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = documentId1,
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };
        
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = documentId2,
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Writer
        };

        var ids = await repository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            DocumentIds = [documentId1]
        };

        // Act
        await repository.DeleteAsync(filter);

        // Assert
        var remainingDocumentUsers = (await repository.GetAsync((DocumentUserFilter)null)).ToArray();
        Assert.True(remainingDocumentUsers.All(documentUser => documentUser.DocumentId != documentId1));
        Assert.Contains(remainingDocumentUsers, documentUser => documentUser.DocumentId == documentId2 && ids[1] == documentUser.Id);
    }
    
    [Fact]
    public async Task DeleteAsync_WithUserIdsFilter_DeletesMatchingDocumentUsers()
    {
        // Arrange
        var repository = GetDocumentUserRepository();

        var userId1 = Int.GetUniqueNumber();
        var userId2 = Int.GetUniqueNumber();

        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = userId1,
            AccessLevel = AccessLevel.Reader
        };
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = userId2,
            AccessLevel = AccessLevel.Writer
        };

        await repository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            UserIds = [userId1]
        };

        // Act
        await repository.DeleteAsync(filter);

        // Assert
        var remainingDocumentUsers = (await repository.GetAsync((DocumentUserFilter)null)).ToArray();
        Assert.True(remainingDocumentUsers.All(documentUser => documentUser.UserId != userId1));
        Assert.Contains(remainingDocumentUsers, documentUser => documentUser.UserId == userId2);
    }
    
    [Fact]
    public async Task DeleteAsync_WithAccessLevelsFilter_DeletesMatchingDocumentUsers()
    {
        // Arrange
        var repository = GetDocumentUserRepository();

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
            AccessLevels = [AccessLevel.Reader]
        };

        // Act
        await repository.DeleteAsync(filter);

        // Assert
        var remainingDocumentUsers = (await repository.GetAsync((DocumentUserFilter)null)).ToArray();
        Assert.True(remainingDocumentUsers.All(documentUser => documentUser.DocumentId != documentId));
    }
    
    [Fact]
    public async Task DeleteAsync_WithCombinedFilters_DeletesMatchingDocumentUsers()
    {
        // Arrange
        var repository = GetDocumentUserRepository();

        var documentId1 = Int.GetUniqueNumber();
        var documentId2 = Int.GetUniqueNumber();
        var userId1 = Int.GetUniqueNumber();
        var userId2 = Int.GetUniqueNumber();

        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = documentId1,
            UserId = userId1,
            AccessLevel = AccessLevel.Reader
        };
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = documentId2,
            UserId = userId2,
            AccessLevel = AccessLevel.Writer
        };

        var ids = await repository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            DocumentIds = [documentId1],
            UserIds = [userId1]
        };

        // Act
        await repository.DeleteAsync(filter);

        // Assert
        var remainingDocumentUsers = (await repository.GetAsync((DocumentUserFilter)null)).ToArray();
        Assert.True(remainingDocumentUsers.All(documentUser => documentUser.DocumentId != documentId1));
        Assert.Contains(
            remainingDocumentUsers, 
            documentUser => documentUser.DocumentId == documentId2 &&
                            documentUser.UserId == userId2 &&
                            documentUser.Id == ids[1]);
    }
    
    private IDocumentUserRepository GetDocumentUserRepository() =>
        new DocumentUserRepository(Options.Create(new PostgresOptions
        {
            ConnectionString = _fixture.ConnectionString
        }));

    private IDocumentRepository GetDocumentRepository() =>
        new DocumentRepository(Options.Create(new PostgresOptions
        {
            ConnectionString = _fixture.ConnectionString
        }));
}