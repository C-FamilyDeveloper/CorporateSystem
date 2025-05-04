using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CorporateSystem.SharedDocs.Api.Requests;
using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Tests.Builders;
using CorporateSystem.SharedDocs.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
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

        var documentId = 1;
        
        var request = new AddUserToDocumentRequest
        {
            DocumentId = documentId,
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
                    DocumentId = documentId,
                    AccessLevel = AccessLevel.Reader
                }
            ]);

        // Act
        var response = await httpClient.PostAsJsonAsync("/api/add-user-to-document", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

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

        var title1 = StringHelper.GetUniqueString();
        var title2 = StringHelper.GetUniqueString();
        
        var userInfo = new UserInfo { Id = 1, Role = "Writer" };
        var documents = new[]
        {
            new DocumentBuilder()
                .WithTitle(title1)
                .Build(),
            new DocumentBuilder()
                .WithTitle(title2)
                .Build()
        };
        
        factory.MockDocumentService
            .Setup(service => service.GetCurrentUserDocuments(
                It.Is<int>(id => id == userInfo.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);
        
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/get-documents-for-current-user");
        request.Headers.Add("X-User-Info", JsonSerializer.Serialize(userInfo));

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadFromJsonAsync<GetDocumentsResponse[]>();
        responseBody.Should().NotBeNull();
        responseBody.Should().HaveCount(2);
        responseBody[0].Title.Should().Be(title1);
        responseBody[1].Title.Should().Be(title2);
        
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
    
    [Fact]
    public async Task CreateDocument_ReturnsCreatedDocument_WhenRequestIsValid()
    {
        // Arrange
        var httpClient = factory.CreateClient();

        var userInfo = new UserInfo { Id = 1, Role = "Writer" };
        var request = new CreateDocumentRequest { Title = "New Document" };
        
        factory.MockDocumentService
            .Setup(service => service.CreateDocumentAsync(
                It.Is<CreateDocumentDto>(dto =>
                    dto.Title == request.Title &&
                    dto.Content == string.Empty &&
                    dto.OwnerId == userInfo.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/create-document")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-User-Info", JsonSerializer.Serialize(userInfo));

        // Act
        var response = await httpClient.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadFromJsonAsync<CreateDocumentResponse>();
        responseBody.Should().NotBeNull();
        responseBody.Id.Should().Be(1);
        responseBody.Title.Should().Be(request.Title);
        responseBody.Content.Should().Be(string.Empty);
        
        factory.MockDocumentService.Verify(
            service => service.CreateDocumentAsync(
                It.Is<CreateDocumentDto>(dto =>
                    dto.Title == request.Title &&
                    dto.Content == string.Empty &&
                    dto.OwnerId == userInfo.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteUserFromDocument_ReturnsOk_WhenUserIsOwner()
    {
        var httpClient = factory.CreateClient();

        var userInfo = new UserInfo { Id = 1, Role = "Writer" };
        var documentId = 1;
        var userId = 2;
        
        factory.MockDocumentService
            .Setup(service => service.GetDocumentAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new DocumentBuilder()
                    .WithId(documentId)
                    .WithOwnerId(userInfo.Id)
                    .Build());
        
        factory.MockDocumentService
            .Setup(service => service.DeleteUsersFromCurrentDocumentAsync(
                It.Is<DeleteUserFromDocumentDto>(dto =>
                    dto.DocumentId == documentId && dto.UserId == userId),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/delete-user-from-document/{documentId}?userId={userId}");
        httpRequest.Headers.Add("X-User-Info", JsonSerializer.Serialize(userInfo));

        // Act
        var response = await httpClient.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        factory.MockDocumentService.Verify(
            service => service.GetDocumentAsync(
                documentId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        factory.MockDocumentService.Verify(
            service => service.DeleteUsersFromCurrentDocumentAsync(
                It.Is<DeleteUserFromDocumentDto>(dto =>
                    dto.DocumentId == documentId && dto.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task DeleteUserFromDocument_ReturnsForbidden_WhenUserIsNotOwner()
    {
        // Arrange
        var httpClient = factory.CreateClient();

        var userInfo = new UserInfo { Id = 2, Role = "Writer" };
        var documentId = 1;
        var userId = 3;
        
        factory.MockDocumentService
            .Setup(service => service.GetDocumentAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new DocumentBuilder()
                    .WithId(documentId)
                    .WithOwnerId(userId)
                    .Build());

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/delete-user-from-document/{documentId}?userId={userId}");
        httpRequest.Headers.Add("X-User-Info", JsonSerializer.Serialize(userInfo));

        // Act
        var response = await httpClient.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("У вас нет прав на выполнение текущей операции");

        factory.MockDocumentService.Verify(
            service => service.GetDocumentAsync(documentId, It.IsAny<CancellationToken>()),
            Times.Once);

        factory.MockDocumentService.Verify(
            service => service.DeleteUsersFromCurrentDocumentAsync(
                It.IsAny<DeleteUserFromDocumentDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    
    [Fact]
    public async Task DeleteUserFromDocument_ReturnsBadRequest_WhenUserInfoIsMissing()
    {
        // Arrange
        var httpClient = factory.CreateClient();

        var documentId = 1;
        var userId = 2;
        
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/delete-user-from-document/{documentId}?userId={userId}");

        factory.MockDocumentService
            .Setup(service =>
                service.GetDocumentAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentBuilder().Build());
        
        // Act
        var response = await httpClient.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Отсутствует информация о пользователе");

        factory.MockDocumentService.Verify(
            service => service.GetDocumentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        factory.MockDocumentService.Verify(
            service => service.DeleteUsersFromCurrentDocumentAsync(
                It.IsAny<DeleteUserFromDocumentDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    
    [Fact]
    public async Task CreateDocument_ReturnsBadRequest_WhenUserInfoIsMissing()
    {
        // Arrange
        var httpClient = factory.CreateClient();

        var request = new CreateDocumentRequest { Title = "New Document" };
        
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/create-document")
        {
            Content = JsonContent.Create(request)
        };

        // Act
        var response = await httpClient.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Отсутствует информация о пользователе");
    }
}