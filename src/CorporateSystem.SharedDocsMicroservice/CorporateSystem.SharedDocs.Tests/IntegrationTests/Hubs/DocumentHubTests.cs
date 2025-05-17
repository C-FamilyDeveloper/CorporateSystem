using System.Text.Json;
using CorporateSystem.SharedDocs.Api.Requests;
using CorporateSystem.SharedDocs.Api.Responses;
using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Services.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Moq;
using UserInfo = CorporateSystem.SharedDocs.Api.Requests.UserInfo;

namespace CorporateSystem.SharedDocs.Tests.IntegrationTests.Hubs;

public class DocumentHubTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly Uri _hubUri = new("http://localhost/document-hub");

   [Fact]
    public async Task TwoUsersCanEditSameDocumentAndReceiveChangeLogs()
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
        var userEmail1 = "someEmail@test.ru";
        
        factory.MockDocumentService
            .Setup(service => service.GetDocumentUsersAsync(
                It.Is<GetDocumentUsersDto>(dto => dto.DocumentId == documentId && dto.UserIds.Contains(user1.Id)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
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
            .ReturnsAsync([
                new DocumentUser
                {
                    UserId = user2.Id,
                    DocumentId = documentId,
                    AccessLevel = AccessLevel.Writer
                }
            ]);
        
        factory.MockDocumentService
            .Setup(service => service.GetDocumentAsync(
                It.Is<int>(id => id == documentId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        
        factory.MockDocumentService
            .Setup(service => service.UpdateDocumentContentAsync(
                It.Is<UpdateDocumentContentDto>(dto => dto.DocumentId == documentId),
                It.IsAny<CancellationToken>()))
            .Returns((UpdateDocumentContentDto dto, CancellationToken _) =>
            {
                document.Content = dto.NewContent;
                return Task.CompletedTask;
            });
        
        factory.MockDocumentChangeLogService
            .Setup(service => service.AddChangeLogAsync(
                It.IsAny<ChangeLog>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        factory.MockDocumentChangeLogService
            .Setup(service => service.GetChangeLogsAsync(
                It.Is<int>(id => id == documentId),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new DocumentChangeLogDto
                {
                    Id = 1,
                    DocumentId = documentId,
                    UserEmail = userEmail1,
                    Changes = "Updated content by User1"
                }
            ]);
        
        var connection1 = CreateHubConnection(user1);
        var connection2 = CreateHubConnection(user2);

        var receivedChangeLogs = new List<ChangeLogResponse>();
        connection2.On<ChangeLogResponse[]>("ReceiveChangeLogs", changeLogs =>
        {
            receivedChangeLogs.AddRange(changeLogs);
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
        
        receivedChangeLogs.Should().HaveCount(1);
        receivedChangeLogs[0].Should().BeEquivalentTo(new ChangeLogResponse
        {
            Id = 1,
            DocumentId = documentId,
            UserEmail = userEmail1,
            Changes = newContent
        });
        
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
                options.Headers.Add("Authorization", "Bearer token");
            })
            .Build();
    }
}