using System.Net.Http.Json;
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
            .Setup(api => api.GetUserIdsByEmailsAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([1, 2]);

        factory.MockDocumentService
            .Setup(service => service.GetDocumentUsersAsync(It.IsAny<GetDocumentUsersDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        factory.MockDocumentService
            .Setup(service => service.AddUsersToDocumentAsync(It.IsAny<AddUserToDocumentDto>(), It.IsAny<CancellationToken>()))
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
            .Setup(service => service.GetDocumentUsersAsync(It.IsAny<GetDocumentUsersDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new DocumentUser { UserId = 1, DocumentId = 1, AccessLevel = AccessLevel.Reader }
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
}