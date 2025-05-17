using CorporateSystem.SharedDocs.Api.Requests;
using CorporateSystem.SharedDocs.Api.Responses;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Exceptions;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using UserInfo = CorporateSystem.SharedDocs.Api.Requests.UserInfo;

namespace CorporateSystem.SharedDocs.Api.Hubs;

public class DocumentHub(
    IDocumentService documentService,
    ILogger<DocumentHub> logger,
    IHttpContextAccessor httpContextAccessor,
    IDocumentChangeLogService documentChangeLogService) : Hub
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

        if (httpContext?.Items["Authorization"] is not string token)
        {
            logger.LogError($"{nameof(OnConnectedAsync)}: ['Authorization']={httpContext?.Items["Authorization"]}");
            throw new ArgumentException("Отсутствует токен");
        }
        
        Context.Items["X-User-Info"] = userInfo;
        Context.Items["Authorization"] = token;

        await base.OnConnectedAsync();
        logger.LogInformation($"Connection for connectionId={Context.ConnectionId} completed");
    }
    
    public async Task<JoinDocumentGroupResponse> JoinDocumentGroup(JoinDocumentGroupRequest request)
    {
        var userInfo = GetUserInfoOrThrowException();
        
        await ThrowIfUserDontHavePermissionsToEditCurrentDocument(request.DocumentId, userInfo.Id, Context.ConnectionAborted);
        
        var document = await documentService.GetDocumentAsync(request.DocumentId);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, request.DocumentId.ToString());
        await Clients.Caller.SendAsync("ReceiveDocumentContent", document.Content);

        var logs = await GetChangeLogsForDocumentAsync(request.DocumentId, Context.ConnectionAborted);
        
        return new JoinDocumentGroupResponse
        {
            Content = document.Content,
            ChangeLogs = logs
        };
    }
    
    public async Task SendDocumentUpdate(SendDocumentUpdateRequest request)
    {
        if (request == null)
        {
            logger.LogError($"{nameof(SendDocumentUpdate)}: Request=null");
            throw new ArgumentException("Что-то пошло не так");
        }

        var userInfo = GetUserInfoOrThrowException();

        await ThrowIfUserDontHavePermissionsToEditCurrentDocument(request.DocumentId, userInfo.Id, Context.ConnectionAborted);
        
        await documentService.UpdateDocumentContentAsync(new UpdateDocumentContentDto
        {
            DocumentId = request.DocumentId,
            UserId = userInfo.Id,
            NewContent = request.NewContent,
            Line = request.Line
        }, Context.ConnectionAborted);

        var logResponses = await GetChangeLogsForDocumentAsync(
            request.DocumentId,
            Context.ConnectionAborted);
        
        await Clients.Group(request.DocumentId.ToString()).SendAsync("ReceiveChangeLogs", logResponses);
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

    private async Task<ChangeLogResponse[]> GetChangeLogsForDocumentAsync(
        int documentId,
        CancellationToken cancellationToken)
    {
        var token = Context.Items["Authorization"].ToString();
        if (string.IsNullOrEmpty(token))
        {
            logger.LogError($"{nameof(SendDocumentUpdate)}: Authorization token is missing");
            throw new UnauthorizedAccessException("Токен аутентификации отсутствует");
        }
        
        var logs = await documentChangeLogService
            .GetChangeLogsAsync(documentId, token, cancellationToken);
        
        var logResponses = logs.Select(log => new ChangeLogResponse
        {
            Id = log.Id,
            DocumentId = log.DocumentId,
            UserEmail = log.UserEmail,
            Changes = log.Changes,
            ChangedAt = log.ChangedAt
        }).ToArray();

        return logResponses;
    }

    private async Task ThrowIfUserDontHavePermissionsToEditCurrentDocument(
        int documentId,
        int userId,
        CancellationToken cancellationToken)
    {
        // todo: можно подумать о кэшировании
        var users = await documentService.GetDocumentUsersAsync(
            new GetDocumentUsersDto(documentId, [userId]),
            cancellationToken);

        var usersArray = users.ToArray();
        
        if (!usersArray.Any())
        {
            logger.LogError($"{nameof(ThrowIfUserDontHavePermissionsToEditCurrentDocument)}: User with ID {userId} not found in document {documentId}");
            throw new InsufficientPermissionsException("У вас нет доступа к данному файлу");
        }
    }
}