using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Services.Implementations;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using CorporateSystem.SharedDocs.Tests.Helpers;
using Moq;

namespace CorporateSystem.SharedDocs.Tests.IntegrationTests.Services;

public class DocumentChangeLogServiceTests
{
    private readonly Mock<IDocumentChangeLogRepository> _mockDocumentChangeLogRepository;
    private readonly Mock<IAuthApiService> _mockAuthApiService;
    private readonly DocumentChangeLogService _documentChangeLogService;

    public DocumentChangeLogServiceTests()
    {
        _mockDocumentChangeLogRepository = new Mock<IDocumentChangeLogRepository>();
        _mockAuthApiService = new Mock<IAuthApiService>();

        _documentChangeLogService = new DocumentChangeLogService(
            _mockDocumentChangeLogRepository.Object,
            _mockAuthApiService.Object);
    }
    
    [Fact]
    public async Task AddChangeLogAsync_CreatesNewLog_WhenNoExistingLogs()
    {
        // Arrange
        var changeLog = new ChangeLog
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            Line = Int.GetUniqueNumber(),
            Changes = StringHelper.GetUniqueString()
        };

        _mockDocumentChangeLogRepository
            .Setup(repo => 
                repo.GetAsync(It.IsAny<DocumentChangeLogFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _documentChangeLogService.AddChangeLogAsync(changeLog);

        // Assert
        _mockDocumentChangeLogRepository.Verify(repo => repo.CreateAsync(
            It.IsAny<CreateDocumentChangeLogDto[]>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task AddChangeLogAsync_UpdatesExistingLog_WhenChangesAreRelated()
    {
        // Arrange
        var userId = Int.GetUniqueNumber();
        var documentId = Int.GetUniqueNumber();
        var line = Int.GetUniqueNumber();
        
        var changeLog = new ChangeLog
        {
            DocumentId = documentId,
            UserId = userId,
            Line = line,
            Changes = "Updated changes"
        };

        var existingLog = new DocumentChangeLog
        {
            Id = Int.GetUniqueNumber(),
            UserId = userId,
            DocumentId = documentId,
            Line = line,
            Changes = "Initial changes",
            ChangedAt = DateTimeOffset.UtcNow
        };

        _mockDocumentChangeLogRepository
            .Setup(repo => 
                repo.GetAsync(It.IsAny<DocumentChangeLogFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingLog]);

        // Act
        await _documentChangeLogService.AddChangeLogAsync(changeLog);

        // Assert
        _mockDocumentChangeLogRepository
            .Verify(repo =>
                repo.UpdateAsync(
                    existingLog.Id,
                It.IsAny<UpdateDocumentChangeLogDto>(),
            It.IsAny<CancellationToken>()),
                Times.Once);
    }
    
    [Fact]
    public async Task AddChangeLogAsync_CreatesNewLog_WhenChangesAreNotRelated()
    {
        // Arrange
        var changeLog = new ChangeLog
        {
            DocumentId = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            Line = Int.GetUniqueNumber(),
            Changes = "Unrelated changes"
        };

        var existingLog = new DocumentChangeLog
        {
            Id = Int.GetUniqueNumber(),
            UserId = Int.GetUniqueNumber(),
            DocumentId = Int.GetUniqueNumber(),
            Line = Int.GetUniqueNumber(),
            Changes = "Initial changes",
            ChangedAt = DateTimeOffset.UtcNow
        };

        _mockDocumentChangeLogRepository
            .Setup(repo => 
                repo.GetAsync(It.IsAny<DocumentChangeLogFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingLog]);

        // Act
        await _documentChangeLogService.AddChangeLogAsync(changeLog);

        // Assert
        _mockDocumentChangeLogRepository.Verify(repo => 
            repo.CreateAsync(
            It.IsAny<CreateDocumentChangeLogDto[]>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetChangeLogsAsync_ReturnsLogs_WithPagination()
    {
        // Arrange
        var documentId = Int.GetUniqueNumber();
        var userId = Int.GetUniqueNumber();
        var userEmail = "user@example.com";

        var logs = Enumerable.Range(1, 60).Select(i => new DocumentChangeLog
        {
            Id = i,
            DocumentId = documentId,
            UserId = userId,
            Changes = $"Change {i}",
            Line = i,
            ChangedAt = DateTimeOffset.UtcNow
        }).ToList();

        _mockDocumentChangeLogRepository
            .Setup(repo => 
                repo.GetAsync(It.IsAny<DocumentChangeLogFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        _mockAuthApiService
            .Setup(api => api.GetUserEmailsByIdsAsync(It.IsAny<int[]>(), It.IsAny<string?>(),It.IsAny<CancellationToken>()))
            .ReturnsAsync([userEmail]);

        // Act
        var result = await _documentChangeLogService.GetChangeLogsAsync(documentId, string.Empty);

        // Assert
        Assert.Equal(50, result.Length); // Проверяем пагинацию (первые 50 записей)
        Assert.All(result.Take(50), log => log.UserEmail.Equals(userEmail));
    }
    
    [Fact]
    public async Task GetChangeLogsAsync_ReturnsCorrectData()
    {
        // Arrange
        var documentId = Int.GetUniqueNumber();
        var userId = Int.GetUniqueNumber();
        var userEmail = "user@example.com";

        var logs = new[]
        {
            new DocumentChangeLog
            {
                Id = Int.GetUniqueNumber(),
                DocumentId = documentId,
                UserId = userId,
                Changes = "Change 1",
                Line = Int.GetUniqueNumber(),
                ChangedAt = DateTimeOffset.UtcNow
            },
            new DocumentChangeLog
            {
                Id = Int.GetUniqueNumber(),
                DocumentId = documentId,
                UserId = userId,
                Changes = "Change 2",
                Line = Int.GetUniqueNumber(),
                ChangedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
            }
        };

        _mockDocumentChangeLogRepository
            .Setup(repo => 
                repo.GetAsync(It.IsAny<DocumentChangeLogFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        _mockAuthApiService
            .Setup(api => api.GetUserEmailsByIdsAsync(It.IsAny<int[]>(), It.IsAny<string?>(),It.IsAny<CancellationToken>()))
            .ReturnsAsync([userEmail]);

        // Act
        var result = await _documentChangeLogService.GetChangeLogsAsync(documentId, string.Empty);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal("Change 1", result[0].Changes);
        Assert.Equal("Change 2", result[1].Changes);
        Assert.Equal(userEmail, result[0].UserEmail);
        Assert.Equal(userEmail, result[1].UserEmail);
    }
}