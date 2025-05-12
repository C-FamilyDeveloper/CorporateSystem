using CorporateSystem.SharedDocs.Api.Requests;
using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Exceptions;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using UserInfo = CorporateSystem.SharedDocs.Api.Requests.UserInfo;

namespace CorporateSystem.SharedDocs.Api.Hubs;

public class DocumentHub(
    IDocumentService documentService,
    ILogger<DocumentHub> logger,
    IHttpContextAccessor httpContextAccessor) : Hub
{
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation($"{nameof(OnConnectedAsync)}: connectionId={Context.ConnectionId}");
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Items["X-User-Info"] is not UserInfo userInfo)
        {
            logger.LogError($"{nameof(OnConnectedAsync)}: UserInfo is missing");
            throw new ArgumentException("Отсутствует информация о пользователе");
        }
        
        Context.Items["X-User-Info"] = userInfo;

        await base.OnConnectedAsync();
        logger.LogInformation($"Connection for connectionId={Context.ConnectionId} completed");
    }
    
    public async Task JoinDocumentGroup(JoinDocumentGroupRequest request)
    {
        var userInfo = GetUserInfoOrThrowException();
        var existingUser =
            await documentService.GetDocumentUsersAsync(new GetDocumentUsersDto(request.DocumentId, [userInfo.Id]),
                Context.ConnectionAborted);
        
        var document = await documentService.GetDocumentAsync(request.DocumentId);
        
        if (!existingUser.Any())
        {
            logger.LogError($"{nameof(JoinDocumentGroup)}: User with ID {userInfo.Id} not found in document {request.DocumentId}");
            throw new InsufficientPermissionsException("У вас нет доступа к данному файлу");
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, request.DocumentId.ToString());
        
        await Clients.Caller.SendAsync("ReceiveDocumentContent", document.Content);
    }
    
    public async Task SendDocumentUpdate(SendDocumentUpdateRequest request)
    {
        if (request == null)
        {
            logger.LogError($"{nameof(SendDocumentUpdate)}: Request=null");
            throw new ArgumentException("Что-то пошло не так");
        }

        var userInfo = GetUserInfoOrThrowException();
        
        var existingUser = await documentService.GetDocumentUsersAsync(
            new GetDocumentUsersDto(request.DocumentId, [userInfo.Id]),
            Context.ConnectionAborted);

        var existingUserArray = existingUser.ToArray();
        
        if (!existingUserArray.Any())
        {
            logger.LogError($"{nameof(SendDocumentUpdate)}: User with ID {userInfo.Id} not found in document {request.DocumentId}");
            throw new InsufficientPermissionsException("У вас нет доступа к данному файлу");
        }

        var currentUser = existingUserArray.Single();
        if (currentUser.AccessLevel != AccessLevel.Writer)
        {
            logger.LogError($"{nameof(SendDocumentUpdate)}: User with ID {userInfo.Id} does not have Writer access to document {request.DocumentId}");
            throw new InsufficientPermissionsException("У вас нет прав на редактирование данного файла");
        }
        
        await documentService.UpdateDocumentContentAsync(new UpdateDocumentContentDto
        {
            DocumentId = request.DocumentId,
            UserId = userInfo.Id,
            NewContent = request.NewContent
        }, Context.ConnectionAborted);
        
        await Clients.Group(request.DocumentId.ToString()).SendAsync("ReceiveDocumentUpdate", request.NewContent); 
    }

    private UserInfo GetUserInfoOrThrowException()
    {
        var userInfo = Context.Items["X-User-Info"] as UserInfo;
        
        if (userInfo != null) 
            return userInfo;
        
        logger.LogError($"{nameof(GetUserInfoOrThrowException)}: UserInfo is missing");
        throw new ArgumentException("Отсутствует информация о пользователе");
    }
}