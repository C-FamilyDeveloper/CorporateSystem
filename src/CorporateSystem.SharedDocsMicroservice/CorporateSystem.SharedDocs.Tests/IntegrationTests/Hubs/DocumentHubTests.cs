using System.Text;
using System.Text.Json;
using CorporateSystem.SharedDocs.Api.Requests;
using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
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
        var document = new Document
        {
            Title = "SomeTitle",
            Content = string.Empty,
            CreatedAt = DateTimeOffset.Now,
            Id = documentId,
            ModifiedAt = null
        };
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

        factory.MockDocumentService
            .Setup(service =>
                service.GetDocumentAsync(
                    It.Is<int>(id => id == documentId),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        factory.MockDocumentService
            .Setup(service =>
                service.UpdateDocumentContentAsync(
                    It.Is<UpdateDocumentContentDto>(dto => dto.DocumentId == documentId), 
                    It.IsAny<CancellationToken>()))
            .Returns((UpdateDocumentContentDto dto, CancellationToken _) =>
            {
                document.Content = dto.NewContent;
                return Task.CompletedTask;
            });
        
        var connection1 = CreateHubConnection(user1);
        var connection2 = CreateHubConnection(user2);
        
        connection2.On<string>("ReceiveDocumentUpdate", newContent =>
        {
            document.Content = newContent;
        });

        // Act
        await connection1.StartAsync();
        await connection2.StartAsync();
        
        await connection1.InvokeAsync("JoinDocumentGroup", new JoinDocumentGroupRequest
        {
            DocumentId = documentId
        });

        await connection2.InvokeAsync("JoinDocumentGroup", new JoinDocumentGroupRequest
        {
            DocumentId = documentId
        });
        
        var newContent = "Updated content by User1";
        await connection1.InvokeAsync("SendDocumentUpdate", new SendDocumentUpdateRequest
        {
            DocumentId = documentId,
            NewContent = newContent
        });

        // Ждем, пока второй пользователь получит обновление
        await Task.Delay(500);

        // Assert
        document.Content.Should().Be(newContent);
        
        await connection1.StopAsync();
        await connection2.StopAsync();
    }

    private HubConnection CreateHubConnection(UserInfo userInfo)
    {
        return new HubConnectionBuilder()
            .WithUrl(_hubUri, options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.Headers.Add("X-User-Info", JsonSerializer.Serialize(userInfo));
            })
            .Build();
    }
}