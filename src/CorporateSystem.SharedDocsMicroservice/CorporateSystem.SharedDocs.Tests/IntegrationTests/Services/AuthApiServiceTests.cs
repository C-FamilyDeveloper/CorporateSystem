using System.Net.Http.Json;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Options;
using CorporateSystem.SharedDocs.Services.Services.Implementations;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace CorporateSystem.SharedDocs.Tests.IntegrationTests.Services;

public class AuthApiServiceTests
{
    private readonly Mock<IOptions<AuthMicroserviceOptions>> _mockOptions;
    private readonly Mock<ILogger<AuthApiService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthApiService _authApiService;

    public AuthApiServiceTests()
    {
        _mockOptions = new Mock<IOptions<AuthMicroserviceOptions>>();
        _mockOptions.Setup(o => o.Value).Returns(new AuthMicroserviceOptions
        {
            Host = "http://localhost:5000"
        });
        
        _mockLogger = new Mock<ILogger<AuthApiService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _httpClientFactory = mockHttpClientFactory.Object;
        
        _authApiService = new AuthApiService(_mockOptions.Object, _httpClientFactory.CreateClient(), _mockLogger.Object);
    }
    
    [Fact]
    public async Task GetUserEmailsByIdsAsync_ReturnsEmails_WhenRequestSucceeds()
    {
        // Arrange
        var ids = new[] { 1, 2, 3 };
        var expectedEmails = new[] { "user1@example.com", "user2@example.com", "user3@example.com" };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(new GetUserEmailsByIdsResponse
                {
                    UserEmails = expectedEmails
                })
            });

        // Act
        var result = await _authApiService.GetUserEmailsByIdsAsync(ids);

        // Assert
        result.Should().BeEquivalentTo(expectedEmails);
    }
    
    [Fact]
    public async Task GetUserEmailsByIdsAsync_ThrowsException_WhenResponseIsEmpty()
    {
        // Arrange
        var ids = new[] { 1, 2, 3 };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create((GetUserEmailsByIdsResponse)null) // Пустой ответ
            });

        // Act & Assert
        var act = () => _authApiService.GetUserEmailsByIdsAsync(ids);
        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    [Fact]
    public async Task GetUserEmailsByIdsAsync_ThrowsException_WhenResponseIsNotSuccessful()
    {
        // Arrange
        var ids = new[] { 1, 2, 3 };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = JsonContent.Create(new { Message = "Invalid request" })
            });

        // Act & Assert
        var act = () => _authApiService.GetUserEmailsByIdsAsync(ids);
        await act.Should().ThrowAsync<HttpRequestException>();
    }
    
    [Fact]
    public async Task GetUserIdsByEmailsAsync_ReturnsIds_WhenRequestSucceeds()
    {
        // Arrange
        var emails = new[] { "user1@example.com", "user2@example.com" };
        var expectedIds = new[] { 1, 2 };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(new GetUserIdsByEmailsResponse
                {
                    UserIds = expectedIds
                })
            });

        // Act
        var result = await _authApiService.GetUserIdsByEmailsAsync(emails);

        // Assert
        result.Should().BeEquivalentTo(expectedIds);
    }
    
    [Fact]
    public async Task GetUserIdsByEmailsAsync_ThrowsException_WhenResponseIsEmpty()
    {
        // Arrange
        var emails = new[] { "user1@example.com", "user2@example.com" };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create((GetUserIdsByEmailsResponse)null) // Пустой ответ
            });

        // Act & Assert
        var act = () => _authApiService.GetUserIdsByEmailsAsync(emails);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
    
    [Fact]
    public async Task GetUserIdsByEmailsAsync_ThrowsException_WhenResponseIsNotSuccessful()
    {
        // Arrange
        var emails = new[] { "user1@example.com", "user2@example.com" };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = JsonContent.Create(new { Message = "Invalid request" })
            });

        // Act & Assert
        var act = () => _authApiService.GetUserIdsByEmailsAsync(emails);
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}