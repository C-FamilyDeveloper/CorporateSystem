using System.Net;
using CorporateSystem.SharedDocs.Api.Requests;
using CorporateSystem.SharedDocs.Domain.Exceptions;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CorporateSystem.SharedDocs.Api.Hubs;

public class DocumentHub(
    IDocumentService documentService,
    ILogger<DocumentHub> logger) : Hub
{
    public async Task JoinDocumentGroup(JoinDocumentGroupRequest request)
    {
        if (request is null)
        {
            throw new ExceptionWithStatusCode("Что-то пошло не так", HttpStatusCode.BadRequest);
        }
        
        var httpContext = Context.GetHttpContext();
        if (httpContext?.Items["UserInfo"] is not UserInfo userInfo)
        {
            throw new ExceptionWithStatusCode("Отсутствует информация о пользователе", HttpStatusCode.BadRequest);
        }

        var existingUser =
            await documentService.GetDocumentUsersAsync(new GetDocumentUsersDto(request.DocumentId, [userInfo.Id]),
                Context.ConnectionAborted);

        if (!existingUser.Any())
        {
            await documentService.AddUsersToDocumentAsync(
                new AddUserToDocumentDto(
                    request.DocumentId,
                    [new DocumentUserInfo(userInfo.Id, request.AccessLevel)]),
                Context.ConnectionAborted);
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, request.DocumentId.ToString());
    }
    
    public async Task SendDocumentUpdate(SendDocumentUpdateRequest request)
    {
        if (request == null)
        {
            logger.LogError("DocumentHub.SendDocumentUpdate: Request=null");
            throw new ExceptionWithStatusCode("Что-то пошло не так", HttpStatusCode.BadRequest);
        }
        
        await documentService.UpdateDocumentContentAsync(new UpdateDocumentContentDto
        {
            DocumentId = request.DocumentId,
            UserId = request.UserId,
            NewContent = request.NewContent
        }, Context.ConnectionAborted);
        
        await Clients.Group(request.DocumentId.ToString()).SendAsync("ReceiveDocumentUpdate", request.NewContent);
    }
}