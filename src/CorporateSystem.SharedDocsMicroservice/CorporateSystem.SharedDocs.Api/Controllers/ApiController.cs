using CorporateSystem.SharedDocs.Api.Requests;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CorporateSystem.SharedDocs.Api.Controllers;

[ApiController]
[Route("api")]
public class ApiController(IDocumentService documentService, IAuthApiService authApiService) : ControllerBase
{
    [HttpPost("add-user-to-document")]
    public async Task<IActionResult> AddUserToDocument([FromBody] AddUserToDocumentRequest request)
    {
        var userIds = await authApiService.GetUserIdsByEmailsAsync(
            request.DocumentUserInfos
                .Select(documentInfo => documentInfo.UserEmail)
                .ToArray());
        
        var documentUsers =
            await documentService.GetDocumentUsersAsync(new GetDocumentUsersDto(request.DocumentId, userIds));

        var documentUsersArray = documentUsers.ToArray();

        for (var i = 0; i < userIds.Length; i++)
        {
            if (documentUsersArray.Any(user => user.UserId == userIds[i]))
            {
                return BadRequest($"Пользователь {request.DocumentUserInfos[i].UserEmail} уже добавлен в текущий документ");
            }
        }
        
        await documentService.AddUsersToDocumentAsync(
            new AddUserToDocumentDto(
                request.DocumentId, 
                documentUsersArray
                    .Select(documentUser => new DocumentUserInfo(documentUser.UserId, documentUser.AccessLevel))
                    .ToArray()));

        return Ok();
    }

    [HttpGet("get-documents-for-current-user")]
    public async Task<IActionResult> GetDocumentForCurrentUser()
    {
        if (!HttpContext.Items.TryGetValue("UserInfo", out var value))
        {
            return BadRequest("Отсутствует информация о пользователе");
        }

        var userInfo = value as UserInfo;

        if (userInfo == null)
        {
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
}