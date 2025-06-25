using System.Text.Json;
using CorporateSystem.SharedDocs.Api.Responses;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserInfo = CorporateSystem.SharedDocs.Api.Requests.UserInfo;

namespace CorporateSystem.SharedDocs.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/docs/admin")]
public class AdminController(ILogger<AdminController> logger) : ControllerBase
{
    [HttpGet("documents")]
    public async Task<IActionResult> GetDocuments([FromServices] IDocumentService documentService)
    {
        logger.LogInformation($"{nameof(GetDocuments)}: connectionId={HttpContext.Connection.Id}");
        if (!HttpContext.Request.Headers.TryGetValue("X-User-Info", out var userInfoValue))
        {
            logger.LogInformation($"{nameof(GetDocuments)}: X-User-Info отсутствует");
            return BadRequest("Отсутствует информация о пользователе");
        }
        
        var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoValue);

        if (userInfo == null)
        {
            logger.LogInformation($"{nameof(GetDocuments)}: userInfo=null");
            return BadRequest("Что-то пошло не так");
        }
        
        var documents = await documentService.GetDocumentsAsync();
        var documentsArray = documents.ToArray();

        var response = documentsArray.Select(document => new GetDocumentsForAdminResponse
        {
            Title = document.Title,
            OwnerEmail = document.OwnerEmail,
            CreatedAt = document.CreatedAt,
            ModifiedAt = document.ModifiedAt,
            Id = document.Id
        }).ToArray();

        return Ok(response);
    }

    [HttpGet("documents/content/{documentId:int}")]
    public async Task<IActionResult> GetDocumentContent(
        [FromRoute] int documentId,
        [FromServices] IDocumentService documentService)
    {
        logger.LogInformation($"{nameof(GetDocumentContent)}: connectionId={HttpContext.Connection.Id}");
        if (!HttpContext.Request.Headers.TryGetValue("X-User-Info", out var userInfoValue))
        {
            logger.LogInformation($"{nameof(GetDocumentContent)}: X-User-Info отсутствует");
            return BadRequest("Отсутствует информация о пользователе");
        }
        
        var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoValue);

        if (userInfo == null)
        {
            logger.LogInformation($"{nameof(GetDocumentContent)}: userInfo=null");
            return BadRequest("Что-то пошло не так");
        }
        
        var document = await documentService.GetDocumentAsync(documentId);

        return Ok(new GetDocumentContentResponse
        {
            Content = document.Content
        });
    }

    [HttpDelete("documents/{documentId:int}")]
    public async Task<IActionResult> DeleteDocument(
        [FromRoute] int documentId,
        [FromServices] IDocumentService documentService)
    {
        logger.LogInformation($"{nameof(GetDocumentContent)}: connectionId={HttpContext.Connection.Id}");
        if (!HttpContext.Request.Headers.TryGetValue("X-User-Info", out var userInfoValue))
        {
            logger.LogInformation($"{nameof(GetDocumentContent)}: X-User-Info отсутствует");
            return BadRequest("Отсутствует информация о пользователе");
        }
        
        var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoValue);

        if (userInfo == null)
        {
            logger.LogInformation($"{nameof(GetDocumentContent)}: userInfo=null");
            return BadRequest("Что-то пошло не так");
        }

        await documentService.DeleteDocumentAsync(new DeleteDocumentDto
        {
            Ids = [documentId]
        });

        return Ok();
    }
}