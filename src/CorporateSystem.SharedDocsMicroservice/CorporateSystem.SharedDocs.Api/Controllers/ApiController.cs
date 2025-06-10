using System.Net;
using System.Text.Json;
using CorporateSystem.SharedDocs.Api.Requests;
using CorporateSystem.SharedDocs.Api.Responses;
using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Exceptions;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using UserInfo = CorporateSystem.SharedDocs.Api.Requests.UserInfo;

namespace CorporateSystem.SharedDocs.Api.Controllers;

[ApiController]
[Route("api")]
public class ApiController(
    IDocumentService documentService,
    IAuthApiService authApiService,
    ILogger<ApiController> logger) : ControllerBase
{
    [HttpPost("add-user-to-document")]
    public async Task<IActionResult> AddUserToDocument([FromBody] AddUserToDocumentRequest request)
    {
        logger.LogInformation($"{nameof(AddUserToDocument)}: connectionId={HttpContext.Connection.Id}");
        var userIds = await authApiService.GetUserIdsByEmailsAsync(
            request.DocumentUserInfos
                .Select(documentInfo => documentInfo.UserEmail)
                .ToArray());

        if (!userIds.Any())
        {
            logger.LogInformation($"Попытка добавить несуществующего пользователя, userInfos={JsonSerializer.Serialize(request.DocumentUserInfos)}");
            return BadRequest("Один или несколько из пользователей не были найдены");
        }
        
        logger.LogInformation($"{nameof(AddUserToDocument)}: userIds={string.Join(",", userIds)}");
        var documentUsers =
            await documentService.GetDocumentUsersAsync(new GetDocumentUsersDto(request.DocumentId, userIds));

        var documentUsersArray = documentUsers.ToArray();

        for (var i = 0; i < userIds.Length; i++)
        {
            if (documentUsersArray.Any(user => user.UserId == userIds[i]))
            {
                logger.LogError($"{nameof(AddUserToDocument)}: Пользователь {request.DocumentUserInfos[i].UserEmail} уже добавлен в текущий документ id={request.DocumentId}");
                return BadRequest($"Пользователь {request.DocumentUserInfos[i].UserEmail} уже добавлен в текущий документ");
            }
        }
        
        logger.LogInformation($"{nameof(AddUserToDocument)}: Все пользователи прошли валидацию");
        
        await documentService.AddUsersToDocumentAsync(
            new AddUserToDocumentDto(
                request.DocumentId, 
                userIds
                    .Select(id => new DocumentUserInfo(id, AccessLevel.Writer))
                    .ToArray()));

        logger.LogInformation($"{nameof(AddUserToDocument)}: Все пользователи были успешно добавлены в БД");
        return Ok();
    }

    [HttpGet("get-documents-for-current-user")]
    public async Task<IActionResult> GetDocumentForCurrentUser()
    {
        logger.LogInformation($"{nameof(GetDocumentForCurrentUser)}: connectionId={HttpContext.Connection.Id}");
        if (!HttpContext.Request.Headers.TryGetValue("X-User-Info", out var value))
        {
            logger.LogInformation($"{nameof(GetDocumentForCurrentUser)}: X-User-Info отсутствует");
            return BadRequest("Отсутствует информация о пользователе");
        }

        var userInfo = JsonSerializer.Deserialize<UserInfo>(value);

        if (userInfo == null)
        {
            logger.LogInformation($"{nameof(GetDocumentForCurrentUser)}: userInfo=null");
            return BadRequest("Что-то пошло не так");
        }

        var userId = userInfo.Id;
        var documents = await documentService.GetCurrentUserDocuments(userId);

        var documentsArray = documents.ToArray();

        var result = documentsArray
            .Select(document => new GetDocumentsResponse
            {
                Id = document.Id,
                Title = document.Title,
                IsOwner = document.IsOwner
            })
            .ToArray();

        return Ok(result);
    }

    [HttpGet("documents/users")]
    public async Task<IActionResult> GetUsersOfCurrentDocument([FromQuery] int documentId)
    {
        var userInfos = await documentService.GetUsersOfCurrentDocument(documentId);

        var response = userInfos.Select(userInfo => new GetUsersOfCurrentDocument
        {
            Email = userInfo.Email
        });
        
        return Ok(response);
    }
    
    [HttpPost("create-document")]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequest request)
    {
        logger.LogInformation($"{nameof(CreateDocument)}: connectionId={HttpContext.Connection.Id}");
        if (!HttpContext.Request.Headers.TryGetValue("X-User-Info", out var userInfoValue))
        {
            logger.LogInformation($"{nameof(CreateDocument)}: X-User-Info отсутствует");
            return BadRequest("Отсутствует информация о пользователе");
        }

        var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoValue);

        if (userInfo == null)
        {
            logger.LogInformation($"{nameof(CreateDocument)}: userInfo=null");
            return BadRequest("Что-то пошло не так");
        }
            
        var createDocumentDto = new CreateDocumentDto
        {
            Title = request.Title,
            Content = string.Empty, // Content всегда пустой при создании
            OwnerId = userInfo.Id
        };

        var createdDocumentId = await documentService.CreateDocumentAsync(createDocumentDto);

        var response = new CreateDocumentResponse
        {
            Id = createdDocumentId,
            Title = request.Title,
            Content = string.Empty
        };

        return Ok(response);
    }

    [HttpDelete("documents/{documentId:int}")]
    public async Task<IActionResult> DeleteDocument([FromRoute] int documentId)
    {
        logger.LogInformation($"{nameof(DeleteDocument)}: connectionId={HttpContext.Connection.Id}");
        if (!HttpContext.Request.Headers.TryGetValue("X-User-Info", out var userInfoValue))
        {
            logger.LogInformation($"{nameof(DeleteDocument)}: X-User-Info отсутствует");
            return BadRequest("Отсутствует информация о пользователе");
        }

        var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoValue);

        if (userInfo == null)
        {
            logger.LogInformation($"{nameof(CreateDocument)}: userInfo=null");
            return BadRequest("Что-то пошло не так");
        }
            
        await documentService.DeleteDocumentAsync(new DeleteDocumentDto(Ids: [documentId]));

        return Ok();
    }

    [HttpDelete("documents/users/{userId:int}")]
    public async Task<IActionResult> DeleteDocumentByUserId([FromRoute] int userId)
    {
        logger.LogInformation($"{nameof(DeleteDocument)}: connectionId={HttpContext.Connection.Id}");
        if (!HttpContext.Request.Headers.TryGetValue("X-User-Info", out var userInfoValue))
        {
            logger.LogInformation($"{nameof(DeleteDocument)}: X-User-Info отсутствует");
            return BadRequest("Отсутствует информация о пользователе");
        }
        
        var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoValue);

        if (userInfo == null)
        {
            logger.LogInformation($"{nameof(CreateDocument)}: userInfo=null");
            return BadRequest("Что-то пошло не так");
        }

        var deleteDocumentTask = documentService.DeleteDocumentAsync(new DeleteDocumentDto(OwnerIds: [userId]));
        var deleteDocumentUserTask = documentService.DeleteDocumentUsersAsync(new DeleteDocumentUsersDto(UserIds: [userId]));

        await Task.WhenAll(deleteDocumentTask, deleteDocumentUserTask);
        
        return Ok();
    }
    
    [HttpDelete("delete-user-from-document/{documentId:int}")]
    public async Task<IActionResult> DeleteUserFromDocument([FromRoute] int documentId, [FromQuery] string userEmail)
    {
        logger.LogInformation($"{nameof(DeleteUserFromDocument)}: connectionId={HttpContext.Connection.Id}");
        if (!HttpContext.Request.Headers.TryGetValue("X-User-Info", out var userInfoValue))
        {
            logger.LogInformation($"{nameof(DeleteUserFromDocument)}: X-User-Info отсутствует");
            return BadRequest("Отсутствует информация о пользователе");
        }
        
        var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoValue);

        if (userInfo == null)
        {
            logger.LogInformation($"{nameof(DeleteUserFromDocument)}: userInfo=null");
            return BadRequest("Что-то пошло не так");
        }
            
        var currentDocument = await documentService.GetDocumentAsync(documentId);

        if (currentDocument.OwnerId != userInfo.Id)
        {
            logger.LogInformation(
                $"{nameof(DeleteUserFromDocument)}: Текущий пользователь пытается удалить пользователя с email={userEmail}, не являясь владельцем документа с id={documentId}");

            throw new InsufficientPermissionsException("У вас нет прав на выполнение текущей операции");
        }

        var userIdResponse = await authApiService.GetUserIdsByEmailsAsync([userEmail]);
            
        var userId = userIdResponse.Single();

        if (userId == userInfo.Id)
        {
            logger.LogInformation($"{nameof(DeleteUserFromDocument)}: Пользователь с id={userInfo.Id} пытается удалить самого себя из документа в котором он не является владельцем");
            return BadRequest("Что-то пошло не так");
        }
            
        await documentService.DeleteUsersFromCurrentDocumentAsync(
            new DeleteUserFromDocumentDto(documentId, userId));

        return Ok();
    }
}