using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;
using CorporateSystem.SharedDocs.Tests.Helpers;

namespace CorporateSystem.SharedDocs.Tests.IntegrationTests.Repositories;

[Collection("PostgresCollection")]
public class DocumentChangeLogRepositoryTests : BaseRepositoryTests
{
    public DocumentChangeLogRepositoryTests(PostgresContainer postgresContainer) : base(postgresContainer)
    {
    }

    [Fact]
    public async Task GetAsync_WithoutFilter_ReturnsAllLogs()
    {
        // Arrange
        var documentRepository = GetDocumentRepository();
        var documentLogsRepository = GetDocumentChangeLogRepository();

        var documentId = (await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Title = StringHelper.GetUniqueString()
            }
        ])).Single();

        var logIds = await documentLogsRepository.CreateAsync([
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId,
                UserId = Int.GetUniqueNumber(),
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            },
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId,
                UserId = Int.GetUniqueNumber(),
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            }
        ]);

        // Act
        var logs = (await documentLogsRepository.GetAsync()).ToArray();
        
        // Assert
        Assert.All(logIds, id => logs.Any(log => log.Id == id));
    }
    
    [Fact]
    public async Task GetAsync_WithUserIdsFilter_ReturnsFilteredLogs()
    {
        // Arrange
        var documentRepository = GetDocumentRepository();
        var documentLogsRepository = GetDocumentChangeLogRepository();

        var documentId = (await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Title = StringHelper.GetUniqueString()
            }
        ])).Single();

        var userId1 = Int.GetUniqueNumber();
        var userId2 = Int.GetUniqueNumber();

        var logIds1 = await documentLogsRepository.CreateAsync([
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId,
                UserId = userId1,
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            }
        ]);

        var logIds2 = await documentLogsRepository.CreateAsync([
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId,
                UserId = userId2,
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            }
        ]);

        var filter = new DocumentChangeLogFilter
        {
            UserIds = [userId1]
        };

        // Act
        var logs = (await documentLogsRepository.GetAsync(filter)).ToArray();

        // Assert
        Assert.Equal(logIds1.Length, logs.Length);
        Assert.All(logIds1, id => logs.Any(log => log.Id == id));
        Assert.DoesNotContain(logs, log => logIds2.Contains(log.Id));
    }
    
    [Fact]
    public async Task GetAsync_WithDocumentIdsFilter_ReturnsFilteredLogs()
    {
        // Arrange
        var documentRepository = GetDocumentRepository();
        var documentLogsRepository = GetDocumentChangeLogRepository();

        var documentId1 = (await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Title = StringHelper.GetUniqueString()
            }
        ])).Single();

        var documentId2 = (await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Title = StringHelper.GetUniqueString()
            }
        ])).Single();

        var logIds1 = await documentLogsRepository.CreateAsync([
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId1,
                UserId = Int.GetUniqueNumber(),
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            }
        ]);

        var logIds2 = await documentLogsRepository.CreateAsync([
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId2,
                UserId = Int.GetUniqueNumber(),
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            }
        ]);

        var filter = new DocumentChangeLogFilter
        {
            DocumentIds = [documentId1]
        };

        // Act
        var logs = (await documentLogsRepository.GetAsync(filter)).ToArray();

        // Assert
        Assert.Equal(logIds1.Length, logs.Length);
        Assert.All(logIds1, id => logs.Any(log => log.Id == id));
        Assert.DoesNotContain(logs, log => logIds2.Contains(log.Id));
    }
    
    [Fact]
    public async Task CreateAsync_CreatesLogsSuccessfully()
    {
        // Arrange
        var documentRepository = GetDocumentRepository();
        var documentLogsRepository = GetDocumentChangeLogRepository();

        var documentId = (await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Title = StringHelper.GetUniqueString()
            }
        ])).Single();

        var dtos = new[]
        {
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId,
                UserId = Int.GetUniqueNumber(),
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            },
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId,
                UserId = Int.GetUniqueNumber(),
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            }
        };

        // Act
        var createdLogIds = await documentLogsRepository.CreateAsync(dtos);

        // Assert
        Assert.Equal(2, createdLogIds.Length);

        var logs = await documentLogsRepository.GetAsync();
        Assert.All(createdLogIds, id => logs.Any(log => log.Id == id));
    }
    
    [Fact]
    public async Task UpdateAsync_UpdatesLogSuccessfully()
    {
        // Arrange
        var documentRepository = GetDocumentRepository();
        var documentLogsRepository = GetDocumentChangeLogRepository();

        var documentId = (await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Title = StringHelper.GetUniqueString()
            }
        ])).Single();

        var logIds = await documentLogsRepository.CreateAsync([
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId,
                UserId = Int.GetUniqueNumber(),
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            }
        ]);

        var logIdToUpdate = logIds.First();
        var updatedChanges = StringHelper.GetUniqueString();

        var dto = new UpdateDocumentChangeLogDto
        {
            Changes = updatedChanges
        };

        // Act
        await documentLogsRepository.UpdateAsync(logIdToUpdate, dto);

        // Assert
        var updatedLog = (await documentLogsRepository.GetAsync(new DocumentChangeLogFilter
        {
            Ids = [logIdToUpdate]
        })).Single();
        
        Assert.Equal(updatedChanges, updatedLog.Changes);
    }
    
    [Fact]
    public async Task DeleteAsync_WithIdsFilter_ShouldReturnFilteredLogs()
    {
        // Arrange
        var documentRepository = GetDocumentRepository();
        var documentLogsRepository = GetDocumentChangeLogRepository();

        var ids = await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Title = StringHelper.GetUniqueString()
            }
        ]);

        var logIds = await documentLogsRepository.CreateAsync([
            new CreateDocumentChangeLogDto
            {
                DocumentId = ids[0],
                UserId = Int.GetUniqueNumber(),
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            }
        ]);
        
        // Act
        await documentLogsRepository.DeleteAsync(new DocumentChangeLogFilter
        {
            Ids = [logIds.Single()]
        });

        // Assert
        Assert.True((await documentLogsRepository.GetAsync()).All(log => log.Id != logIds.Single()));
    }
    
    [Fact]
    public async Task DeleteAsync_WithDocumentIdsFilter_DeletesMatchingLogs()
    {
        // Arrange
        var documentRepository = GetDocumentRepository();
        var documentLogsRepository = GetDocumentChangeLogRepository();

        var documentId1 = (await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Title = StringHelper.GetUniqueString()
            }
        ])).Single();

        var documentId2 = (await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Title = StringHelper.GetUniqueString()
            }
        ])).Single();

        var logIds1 = await documentLogsRepository.CreateAsync([
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId1,
                UserId = Int.GetUniqueNumber(),
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            }
        ]);

        var logIds2 = await documentLogsRepository.CreateAsync([
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId2,
                UserId = Int.GetUniqueNumber(),
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            }
        ]);

        var filter = new DocumentChangeLogFilter
        {
            DocumentIds = [documentId1]
        };

        // Act
        await documentLogsRepository.DeleteAsync(filter);

        // Assert
        var remainingLogs = (await documentLogsRepository.GetAsync()).ToArray();
        Assert.True(logIds1.All(log => remainingLogs.All(log1 => log1.Id != log)));
        Assert.All(logIds2, id => remainingLogs.Any(log => log.Id == id));
    }
    
    [Fact]
    public async Task DeleteAsync_WithUserIdsFilter_DeletesMatchingLogs()
    {
        // Arrange
        var documentRepository = GetDocumentRepository();
        var documentLogsRepository = GetDocumentChangeLogRepository();

        var documentId = (await documentRepository.CreateAsync([
            new CreateDocumentDto
            {
                OwnerId = Int.GetUniqueNumber(),
                Title = StringHelper.GetUniqueString()
            }
        ])).Single();

        var userId1 = Int.GetUniqueNumber();
        var userId2 = Int.GetUniqueNumber();

        var logIds1 = await documentLogsRepository.CreateAsync([
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId,
                UserId = userId1,
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            }
        ]);

        var logIds2 = await documentLogsRepository.CreateAsync([
            new CreateDocumentChangeLogDto
            {
                DocumentId = documentId,
                UserId = userId2,
                Changes = StringHelper.GetUniqueString(),
                Line = Int.GetUniqueNumber()
            }
        ]);

        var filter = new DocumentChangeLogFilter
        {
            UserIds = [userId1]
        };

        // Act
        await documentLogsRepository.DeleteAsync(filter);

        // Assert
        var remainingLogs = await documentLogsRepository.GetAsync();
        Assert.True(logIds1.All(log => remainingLogs.All(log1 => log1.Id != log)));
        Assert.All(logIds2, id => remainingLogs.Any(log => log.Id == id));
    }
}