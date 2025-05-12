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
public class DocumentCompositeRepositoryTests : BaseRepositoryTests
{
    public DocumentCompositeRepositoryTests(PostgresContainer postgresContainer) : base(postgresContainer)
    {
    }

    [Fact]
    public async Task GetAsync_WithOwnerIdsFilter_ReturnsFilteredDocuments()
    {
        // Arrange
        var documentCompositeRepository = GetDocumentCompositeRepository();
        var documentUserRepository = GetDocumentUserRepository();
        var documentRepository = GetDocumentRepository();

        var ownerId1 = Int.GetUniqueNumber();
        var ownerId2 = Int.GetUniqueNumber();
        var title = StringHelper.GetUniqueString();

        var document1 = new CreateDocumentDto
        {
            OwnerId = ownerId1,
            Title = title,
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

        await documentUserRepository.CreateAsync([documentUser1, documentUser2]);
        
        var filter = new DocumentInfoFilter
        {
            OwnerIds = [ownerId1]
        };

        // Act
        var result = (await documentCompositeRepository.GetAsync(filter)).ToArray();

        // Assert
        Assert.Contains(result,
            documentInfo => documentInfo.Id == createdDocumentIds[0] && documentInfo.Title == title);
    }
    
    [Fact]
    public async Task GetAsync_WithFollowerIdsFilter_ReturnsFilteredDocuments()
    {
        // Arrange
        var documentCompositeRepository = GetDocumentCompositeRepository();
        var documentUserRepository = GetDocumentUserRepository();
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

        await documentUserRepository.CreateAsync([createDto1, createDto2]);

        var filter = new DocumentInfoFilter
        {
            FollowerIds = [followerId1]
        };

        // Act
        var result = (await documentCompositeRepository.GetAsync(filter)).ToArray();

        // Assert
        Assert.Single(result);
        var documentInfo = result.First();
        Assert.Equal(documentIds[0], documentInfo.Id);
    }

    [Fact]
    public async Task GetAsync_UserId_ReturnsDocumentInfos()
    {
        // Arrange
        var documentRepository = GetDocumentRepository();
        var documentCompositeRepository = GetDocumentCompositeRepository();
        
        var userId = Int.GetUniqueNumber();

        var documentIds = await documentRepository.CreateAsync(
        [
            new CreateDocumentDto
            {
                OwnerId = userId,
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
        
        // Act
        var result = (await documentCompositeRepository.GetAsync(userId)).ToArray();

        // Assert
        Assert.Single(result);
    }
}