using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CorporateSystem.SharedDocs.Api.Requests;
using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Services.Dtos;
using FluentAssertions;
using Moq;

namespace CorporateSystem.SharedDocs.Tests.IntegrationTests.Api;

public class ApiControllerTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task AddUserToDocument_ReturnsOk_WhenUsersAreSuccessfullyAdded()
    {
        // Arrange
        var httpClient = factory.CreateClient();

        var request = new AddUserToDocumentRequest
        {
            DocumentId = 1,
            DocumentUserInfos =
            [
                new DocumentUserInfoDto { UserEmail = "user1@example.com", AccessLevel = AccessLevel.Reader },
                new DocumentUserInfoDto { UserEmail = "user2@example.com", AccessLevel = AccessLevel.Writer }
            ]
        };
        
        factory.MockAuthApiService
            .Setup(api => api.GetUserIdsByEmailsAsync(
                It.IsAny<string[]>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([1, 2]);

        factory.MockDocumentService
            .Setup(service => service.GetDocumentUsersAsync(
                It.IsAny<GetDocumentUsersDto>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        factory.MockDocumentService
            .Setup(service => service.AddUsersToDocumentAsync(
                It.IsAny<AddUserToDocumentDto>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await httpClient.PostAsJsonAsync("/api/add-user-to-document", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        factory.MockDocumentService.Verify(
            service => service.AddUsersToDocumentAsync(
                It.IsAny<AddUserToDocumentDto>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddUserToDocument_ReturnsBadRequest_WhenUserAlreadyAdded()
    {
        // Arrange
        var httpClient = factory.CreateClient();

        var request = new AddUserToDocumentRequest
        {
            DocumentId = 1,
            DocumentUserInfos =
            [
                new DocumentUserInfoDto { UserEmail = "user1@example.com", AccessLevel = AccessLevel.Reader }
            ]
        };
        
        factory.MockAuthApiService
            .Setup(api => api.GetUserIdsByEmailsAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([1]);

        factory.MockDocumentService
            .Setup(service => service.GetDocumentUsersAsync(
                It.IsAny<GetDocumentUsersDto>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new DocumentUser
                {
                    UserId = 1,
                    DocumentId = 1,
                    AccessLevel = AccessLevel.Reader
                }
            ]);

        // Act
        var response = await httpClient.PostAsJsonAsync("/api/add-user-to-document", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("уже добавлен в текущий документ");
        
        factory.MockDocumentService.Verify(
            service => service.AddUsersToDocumentAsync(
                It.IsAny<AddUserToDocumentDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    
    [Fact]
    public async Task GetDocumentForCurrentUser_ReturnsOk_WithDocuments_ForValidUser()
    {
        // Arrange
        var httpClient = factory.CreateClient();

        var userInfo = new UserInfo { Id = 1, Role = "Writer" };
        var documents = new[]
        {
            new Document
            {
                Id = 1,
                Title = "Doc1",
                ModifiedAt = null,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Document
            {
                Id = 2,
                Title = "Doc2",
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = null
            }
        };
        
        factory.MockDocumentService
            .Setup(service => service.GetCurrentUserDocuments(
                It.Is<int>(id => id == userInfo.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);
        
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/get-documents-for-current-user");
        request.Headers.Add("UserInfo", JsonSerializer.Serialize(userInfo));

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadFromJsonAsync<GetDocumentsResponse[]>();
        responseBody.Should().NotBeNull();
        responseBody.Should().HaveCount(2);
        responseBody[0].Title.Should().Be("Doc1");
        responseBody[1].Title.Should().Be("Doc2");
        
        factory.MockDocumentService.Verify(
            service => service.GetCurrentUserDocuments(
                It.Is<int>(id => id == userInfo.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDocumentForCurrentUser_ReturnsBadRequest_WhenUserInfoIsMissing()
    {
        // Arrange
        var httpClient = factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/get-documents-for-current-user");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Отсутствует информация о пользователе");
    }
}