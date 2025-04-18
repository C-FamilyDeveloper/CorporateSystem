using System.Net;
using CorporateSystem.SharedDocs.Api.Requests;
using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Domain.Exceptions;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CorporateSystem.SharedDocs.Api.Hubs;

public class DocumentHub(
    IDocumentService documentService,
    ILogger<DocumentHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext?.Items["UserInfo"] is not UserInfo userInfo)
        {
            logger.LogError("OnConnectedAsync: UserInfo is missing");
            throw new ExceptionWithStatusCode("Отсутствует информация о пользователе", HttpStatusCode.BadRequest);
        }
        
        Context.Items["UserInfo"] = userInfo;

        await base.OnConnectedAsync();
    }
    
    public async Task JoinDocumentGroup(JoinDocumentGroupRequest request)
    {
        if (request is null)
        {
            throw new ExceptionWithStatusCode("Что-то пошло не так", HttpStatusCode.BadRequest);
        }

        var userInfo = Context.Items["UserInfo"] as UserInfo;
        if (userInfo == null)
        {
            logger.LogError("DocumentHub.SendDocumentUpdate: UserInfo is missing");
            throw new ExceptionWithStatusCode("Отсутствует информация о пользователе", HttpStatusCode.BadRequest);
        }

        var existingUser =
            await documentService.GetDocumentUsersAsync(new GetDocumentUsersDto(request.DocumentId, [userInfo.Id]),
                Context.ConnectionAborted);

        if (!existingUser.Any())
        {
            logger.LogError($"DocumentHub.SendDocumentUpdate: User with ID {userInfo.Id} not found in document {request.DocumentId}");
            throw new ExceptionWithStatusCode("У вас нет доступа к данному файлу", HttpStatusCode.Forbidden);
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
        
        var userInfo = Context.Items["UserInfo"] as UserInfo;
        if (userInfo == null)
        {
            logger.LogError("DocumentHub.SendDocumentUpdate: UserInfo is missing");
            throw new ExceptionWithStatusCode("Отсутствует информация о пользователе", HttpStatusCode.Unauthorized);
        }
        
        var existingUser = await documentService.GetDocumentUsersAsync(
            new GetDocumentUsersDto(request.DocumentId, [userInfo.Id]),
            Context.ConnectionAborted);

        var existingUserArray = existingUser.ToArray();
        
        if (!existingUserArray.Any())
        {
            logger.LogError($"DocumentHub.SendDocumentUpdate: User with ID {userInfo.Id} not found in document {request.DocumentId}");
            throw new ExceptionWithStatusCode("У вас нет доступа к данному файлу", HttpStatusCode.Forbidden);
        }

        var currentUser = existingUserArray.Single();
        if (currentUser.AccessLevel != AccessLevel.Writer)
        {
            logger.LogError($"DocumentHub.SendDocumentUpdate: User with ID {userInfo.Id} does not have Writer access to document {request.DocumentId}");
            throw new ExceptionWithStatusCode("У вас нет прав на редактирование данного файла", HttpStatusCode.Forbidden);
        }
        
        await documentService.UpdateDocumentContentAsync(new UpdateDocumentContentDto
        {
            DocumentId = request.DocumentId,
            UserId = userInfo.Id,
            NewContent = request.NewContent
        }, Context.ConnectionAborted);
        
        await Clients.Group(request.DocumentId.ToString()).SendAsync("ReceiveDocumentUpdate", request.NewContent);
    }
}