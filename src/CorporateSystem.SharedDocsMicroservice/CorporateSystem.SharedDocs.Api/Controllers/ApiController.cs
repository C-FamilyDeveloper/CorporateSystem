using System.Text.Json;
using CorporateSystem.SharedDocs.Api.Requests;
using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        try
        {
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
                    Title = document.Title
                })
                .ToArray();

            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(GetDocumentForCurrentUser)}: {e.Message}");
            return BadRequest("Что-то пошло не так");
        }
    }

    [HttpGet("get-documents-that-user-has-been-invited")]
    public async Task<IActionResult> GetDocumentsThatUserHasBeenInvited()
    {
        logger.LogInformation($"{nameof(GetDocumentsThatUserHasBeenInvited)}: connectionId={HttpContext.Connection.Id}");
        if (!HttpContext.Request.Headers.TryGetValue("X-User-Info", out var value))
        {
            logger.LogInformation($"{nameof(GetDocumentsThatUserHasBeenInvited)}: X-User-Info отсутствует");
            return BadRequest("Отсутствует информация о пользователе");
        }
        
        try
        {
            var userInfo = JsonSerializer.Deserialize<UserInfo>(value);

            if (userInfo == null)
            {
                logger.LogInformation($"{nameof(GetDocumentsThatUserHasBeenInvited)}: userInfo=null");
                return BadRequest("Что-то пошло не так");
            }
            
            var userId = userInfo.Id;
            var documents = await documentService.GetDocumentsThatCurrentUserWasInvitedAsync(userId);

            var documentsArray = documents.ToArray();

            var result = documentsArray
                .Select(document => new GetDocumentsResponse
                {
                    Id = document.Id,
                    Title = document.Title
                })
                .ToArray();

            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError($"{nameof(GetDocumentsThatUserHasBeenInvited)}: {e.Message}");
            return BadRequest("Что-то пошло не так");
        }
    }
    
    [HttpPost("create-document")]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequest request)
    {
        if (!HttpContext.Request.Headers.TryGetValue("X-User-Info", out var userInfoValue))
        {
            logger.LogInformation($"{nameof(CreateDocument)}: X-User-Info отсутствует");
            return BadRequest("Отсутствует информация о пользователе");
        }

        try
        {
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
        catch (Exception e)
        {
            logger.LogError($"{nameof(CreateDocument)}: {e.Message}");
            return BadRequest("Что-то пошло не так");
        }
    }
}