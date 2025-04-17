using System.Text;
using System.Text.Json;
using CorporateSystem.SharedDocs.Api.Requests;
using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Services.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace CorporateSystem.SharedDocs.Tests.IntegrationTests.Hubs;

public class DocumentHubTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly Uri _hubUri = new("http://localhost/document-hub");

    [Fact]
    public async Task TwoUsersCanEditSameDocument()
    {
        // Arrange
        var documentId = 1;
        var user1 = new UserInfo { Id = 1, Role = "Writer" };
        var user2 = new UserInfo { Id = 2, Role = "Writer" };
        
        factory.MockDocumentService
            .Setup(service => service.GetDocumentUsersAsync(
                It.Is<GetDocumentUsersDto>(dto => dto.DocumentId == documentId && dto.UserIds.Contains(user1.Id)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new DocumentUser
                {
                    UserId = user1.Id,
                    DocumentId = documentId,
                    AccessLevel = AccessLevel.Writer
                }
            ]);
        
        factory.MockDocumentService
            .Setup(service => service.GetDocumentUsersAsync(
                It.Is<GetDocumentUsersDto>(dto => dto.DocumentId == documentId && dto.UserIds.Contains(user2.Id)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new DocumentUser
                {
                    UserId = user2.Id,
                    DocumentId = documentId,
                    AccessLevel = AccessLevel.Writer
                }
            ]);
        
        var connection1 = CreateHubConnection(user1);
        var connection2 = CreateHubConnection(user2);
        
        var receivedContentByUser2 = string.Empty;
        
        connection2.On<string>("ReceiveDocumentUpdate", newContent =>
        {
            receivedContentByUser2 = newContent;
        });

        // Act
        await connection1.StartAsync();
        await connection2.StartAsync();
        
        await connection1.InvokeAsync("JoinDocumentGroup", new JoinDocumentGroupRequest
        {
            DocumentId = documentId,
            UserId = user1.Id,
            AccessLevel = AccessLevel.Writer
        });

        await connection2.InvokeAsync("JoinDocumentGroup", new JoinDocumentGroupRequest
        {
            DocumentId = documentId,
            UserId = user2.Id,
            AccessLevel = AccessLevel.Writer
        });
        
        var newContent = "Updated content by User1";
        await connection1.InvokeAsync("SendDocumentUpdate", new SendDocumentUpdateRequest
        {
            DocumentId = documentId,
            UserId = user1.Id,
            NewContent = newContent
        });

        // Ждем, пока второй пользователь получит обновление
        await Task.Delay(500);

        // Assert
        receivedContentByUser2.Should().Be(newContent);
        
        await connection1.StopAsync();
        await connection2.StopAsync();
    }

    private HubConnection CreateHubConnection(UserInfo userInfo)
    {
        return new HubConnectionBuilder()
            .WithUrl(_hubUri, options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.Headers.Add("UserInfo", JsonSerializer.Serialize(userInfo));
            })
            .Build();
    }
}