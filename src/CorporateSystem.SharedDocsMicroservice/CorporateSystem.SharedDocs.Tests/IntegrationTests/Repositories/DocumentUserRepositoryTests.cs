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
public class DocumentUserRepositoryTests : BaseRepositoryTests
{
    public DocumentUserRepositoryTests(PostgresContainer postgresContainer) : base(postgresContainer)
    {
    }

    [Fact]
    public async Task GetAsync_WithExistingId_ReturnsDocumentUser()
    {
        // Arrange
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var documentIds = await documentRepository.CreateAsync(
        [
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            }
        ]);
        
        var documentId = documentIds.Single();
        var userId = Int.GetUniqueNumber();
        
        var createDto = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = userId,
            AccessLevel = AccessLevel.Reader
        };

        var createdIds = await documentUserRepository.CreateAsync([createDto]);
        var documentUserId = createdIds[0];

        // Act
        var result = await documentUserRepository.GetAsync(documentUserId);

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
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var documentIds = await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            },
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            }
        ]);
        
        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = documentIds[0],
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };
        
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = documentIds[1],
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Writer
        };

        var createdIds = await documentUserRepository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            Ids = [createdIds[0]]
        };

        // Act
        var result = (await documentUserRepository.GetAsync(filter)).ToArray();

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
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var documentIds = await documentRepository.CreateAsync(
        [
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            },
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            }
        ]);
        
        var documentId = documentIds[0];
        var userId = Int.GetUniqueNumber();
        
        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = documentIds[1],
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };
        
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = userId,
            AccessLevel = AccessLevel.Writer
        };

        await documentUserRepository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            AccessLevels = [AccessLevel.Writer]
        };

        // Act
        var result = (await documentUserRepository.GetAsync(filter)).ToArray();

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
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var documentIds = await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            },
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            }
        ]);
        
        var documentId = documentIds[0];
        var userId = Int.GetUniqueNumber();
        
        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = documentIds[1],
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };
        
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = userId,
            AccessLevel = AccessLevel.Writer
        };

        await documentUserRepository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            UserIds = [userId]
        };

        // Act
        var result = (await documentUserRepository.GetAsync(filter)).ToArray();

        // Assert
        Assert.Single(result);
        var documentUser = result.First();
        Assert.Equal(userId, documentUser.UserId);
    }
    
    [Fact]
    public async Task CreateAsync_AddsSingleDocumentUserToDatabase()
    {
        // Arrange
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var createDocumentDto = new CreateDocumentDto
        {
            OwnerId = Int.GetUniqueNumber(),
            Content = StringHelper.GetUniqueString(),
            Title = StringHelper.GetUniqueString()
        };

        var documentIds = await documentRepository.CreateAsync([createDocumentDto]);
        
        var documentId = documentIds.Single();
        var userId = Int.GetUniqueNumber();
        
        var createDto = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = userId,
            AccessLevel = AccessLevel.Reader
        };

        // Act
        var ids = await documentUserRepository.CreateAsync([createDto]);

        // Assert
        Assert.Single(ids);
        var documentUser = await documentUserRepository.GetAsync(ids[0]);
        Assert.NotNull(documentUser);
        Assert.Equal(AccessLevel.Reader, documentUser.AccessLevel);
        Assert.Equal(documentId, documentUser.DocumentId);
        Assert.Equal(userId, documentUser.UserId);
    }
    
    [Fact]
    public async Task GetAsync_WithDocumentIdsFilter_ReturnsFilteredDocumentUsers()
    {
        // Arrange
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var documentIds = await documentRepository.CreateAsync(
        [
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            },
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            }
        ]);
        
        var documentId = documentIds[0];
        
        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };
        
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = documentIds[1],
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Writer
        };

        await documentUserRepository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            DocumentIds = [documentId]
        };

        // Act
        var result = (await documentUserRepository.GetAsync(filter)).ToArray();

        // Assert
        Assert.Contains(result,
            documentUser => documentUser.DocumentId == documentId && documentUser.AccessLevel == AccessLevel.Reader);
    }
    
    [Fact]
    public async Task CreateAsync_AddsMultipleDocumentUsersToDatabase()
    {
        // Arrange
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var createDocumentDto1 = new CreateDocumentDto
        {
            OwnerId = Int.GetUniqueNumber(),
            Content = StringHelper.GetUniqueString(),
            Title = StringHelper.GetUniqueString()
        };
        
        var documentIds = await documentRepository.CreateAsync([createDocumentDto1]);
        
        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = documentIds.Single(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = documentIds.Single(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Writer
        };

        // Act
        var ids = await documentUserRepository.CreateAsync([createDto1, createDto2]);

        // Assert
        Assert.Equal(2, ids.Length);

        var documentUser1 = await documentUserRepository.GetAsync(ids[0]);
        Assert.NotNull(documentUser1);
        Assert.Equal(AccessLevel.Reader, documentUser1.AccessLevel);

        var documentUser2 = await documentUserRepository.GetAsync(ids[1]);
        Assert.NotNull(documentUser2);
        Assert.Equal(AccessLevel.Writer, documentUser2.AccessLevel);
    }
    
    [Fact]
    public async Task UpdateAsync_UpdatesDocumentUserInDatabase()
    {
        // Arrange
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var documentIds = await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            }
        ]);
        
        var createDto = new CreateDocumentUserDto
        {
            DocumentId = documentIds.Single(),
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Reader
        };

        var createdIds = await documentUserRepository.CreateAsync([createDto]);
        var documentUserId = createdIds[0];
        
        var newUserId = Int.GetUniqueNumber();
        
        var updateDto = new UpdateDocumentUserDto
        {
            UserId = newUserId,
            AccessLevel = AccessLevel.Writer
        };

        // Act
        await documentUserRepository.UpdateAsync(documentUserId, updateDto);

        // Assert
        var updatedDocumentUser = await documentUserRepository.GetAsync(documentUserId);
        Assert.NotNull(updatedDocumentUser);
        Assert.Equal(AccessLevel.Writer, updatedDocumentUser.AccessLevel);
        Assert.Equal(newUserId, updatedDocumentUser.UserId);
    }
    
    [Fact]
    public async Task DeleteAsync_WithIdsFilter_DeletesMatchingDocumentUsers()
    {
        // Arrange
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var documentIds = await documentRepository.CreateAsync(
    [
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            }
        ]);
        
        var documentId = documentIds.Single();
        var userId = Int.GetUniqueNumber();

        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = userId,
            AccessLevel = AccessLevel.Reader
        };
        
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = documentId,
            UserId = Int.GetUniqueNumber(),
            AccessLevel = AccessLevel.Writer
        };

        var createdIds = await documentUserRepository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            Ids = [createdIds[0]]
        };

        // Act
        await documentUserRepository.DeleteAsync(filter);

        // Assert
        var remainingDocumentUsers = (await documentUserRepository.GetAsync()).ToArray();
        Assert.True(remainingDocumentUsers.All(documentUser => documentUser.Id != createdIds[0]));
        Assert.Contains(remainingDocumentUsers, documentUser => documentUser.Id == createdIds[1]);
    }
    
    [Fact]
    public async Task DeleteAsync_WithDocumentIdsFilter_DeletesMatchingDocumentUsers()
    {
        // Arrange
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var documentIds = await documentRepository.CreateAsync(
        [
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            },
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            },
        ]);
        
        var documentId1 = documentIds[0];
        var documentId2 = documentIds[1];

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

        var ids = await documentUserRepository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            DocumentIds = [documentId1]
        };

        // Act
        await documentUserRepository.DeleteAsync(filter);

        // Assert
        var remainingDocumentUsers = (await documentUserRepository.GetAsync()).ToArray();
        Assert.True(remainingDocumentUsers.All(documentUser => documentUser.DocumentId != documentId1));
        Assert.Contains(remainingDocumentUsers, documentUser => documentUser.DocumentId == documentId2 && ids[1] == documentUser.Id);
    }
    
    [Fact]
    public async Task DeleteAsync_WithUserIdsFilter_DeletesMatchingDocumentUsers()
    {
        // Arrange
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var documentIds = await documentRepository.CreateAsync(
        [
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            }
        ]);
        
        var userId1 = Int.GetUniqueNumber();
        var userId2 = Int.GetUniqueNumber();

        var createDto1 = new CreateDocumentUserDto
        {
            DocumentId = documentIds.Single(),
            UserId = userId1,
            AccessLevel = AccessLevel.Reader
        };
        
        var createDto2 = new CreateDocumentUserDto
        {
            DocumentId = documentIds.Single(),
            UserId = userId2,
            AccessLevel = AccessLevel.Writer
        };

        await documentUserRepository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentUserFilter
        {
            UserIds = [userId1]
        };

        // Act
        await documentUserRepository.DeleteAsync(filter);

        // Assert
        var remainingDocumentUsers = (await documentUserRepository.GetAsync()).ToArray();
        Assert.True(remainingDocumentUsers.All(documentUser => documentUser.UserId != userId1));
        Assert.Contains(remainingDocumentUsers, documentUser => documentUser.UserId == userId2);
    }
    
    [Fact]
    public async Task DeleteAsync_WithCombinedFilters_DeletesMatchingDocumentUsers()
    {
        // Arrange
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var userId1 = Int.GetUniqueNumber();
        var userId2 = Int.GetUniqueNumber();
        
        var documentIds = await documentRepository.CreateAsync(
        [
            new CreateDocumentDto
            {
                OwnerId = userId1,
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            },
            new CreateDocumentDto
            {
                OwnerId = userId2,
                Content = StringHelper.GetUniqueString(),
                Title = StringHelper.GetUniqueString()
            }
        ]);
        
        var documentId1 = documentIds[0];
        var documentId2 = documentIds[1];

        var filter = new DocumentUserFilter
        {
            DocumentIds = [documentId1],
            UserIds = [userId1]
        };

        // Act
        await documentUserRepository.DeleteAsync(filter);

        // Assert
        var remainingDocumentUsers = (await documentUserRepository.GetAsync()).ToArray();
        Assert.True(remainingDocumentUsers.All(documentUser => documentUser.DocumentId != documentId1 && documentUser.UserId != userId1));
        Assert.Contains(
            remainingDocumentUsers, 
            documentUser => documentUser.DocumentId == documentId2 &&
                            documentUser.UserId == userId2);
    }
}