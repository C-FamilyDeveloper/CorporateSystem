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
}