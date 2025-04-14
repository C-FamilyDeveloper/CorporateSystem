using System.Net;
using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Domain.Exceptions;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Options;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CreateDocumentDto = CorporateSystem.SharedDocs.Services.Dtos.CreateDocumentDto;

namespace CorporateSystem.SharedDocs.Services.Services.Implementations;

internal class DocumentService(
    ILogger<DocumentService> logger,
    IDocumentRepository documentRepository,
    IDocumentUserRepository documentUserRepository,
    IAuthApiService authApiService,
    IOptions<AuthMicroserviceOptions> authOptions) : IDocumentService
{
    private readonly AuthMicroserviceOptions _authMicroserviceOptions = authOptions.Value;

    public async Task<int> CreateDocumentAsync(CreateDocumentDto dto, CancellationToken cancellationToken = default)
    {
        var ids = await documentRepository.CreateAsync(
            [new Infrastructure.Dtos.CreateDocumentDto(dto.OwnerId, dto.Title, dto.Content)],
            cancellationToken);

        return ids.Single();
    }

    public async Task AddUsersToDocumentAsync(AddUserToDocumentDto dto, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentOrThrowExceptionAsync(dto.DocumentId, cancellationToken);

        var currentDocumentUsers = await documentUserRepository.GetAsync(new DocumentUserFilter
        {
            DocumentIds = [document.Id]
        }, cancellationToken);
        
        var currentDocumentsUsersList = currentDocumentUsers.ToList();
        
        var currentDocumentUserIds = currentDocumentsUsersList
            .Select(documentUser => documentUser.UserId)
            .ToList();

        var userIdsWhichNeedAdded = new List<DocumentUserInfo>();
        
        foreach (var userInfo in dto.UserInfos)
        {
            if (currentDocumentUserIds.Contains(userInfo.UserId))
            {
                logger.LogError($"Попытка добавить пользователя с идентификатором {userInfo.UserId}, который уже был добавлен ранее к текущему документу");
                throw new ExceptionWithStatusCode(
                    "Попытка добавить уже существующего пользователя",
                    HttpStatusCode.BadRequest);
            }
            
            userIdsWhichNeedAdded.Add(userInfo);
        }
        
        await documentUserRepository
            .CreateAsync(
                userIdsWhichNeedAdded
                    .Select(userInfo => new CreateDocumentUserDto(dto.DocumentId, userInfo.UserId, userInfo.AccessLevel))
                    .ToArray(),
                cancellationToken);
    }

    public async Task<string[]> GetUserEmailsOfCurrentDocumentAsync(
        int documentId,
        CancellationToken cancellationToken = default)
    {
        await GetDocumentOrThrowExceptionAsync(documentId, cancellationToken);

        var documentUsers = await documentUserRepository.GetAsync(new DocumentUserFilter
        {
            DocumentIds = [documentId]
        }, cancellationToken);

        var userIds = documentUsers
            .Select(documentUser => documentUser.UserId)
            .ToArray();

        return await authApiService.GetUserEmailsByIdsAsync(userIds, cancellationToken);
    }

    public Task<IEnumerable<DocumentUser>> GetDocumentUsersAsync(
        GetDocumentUsersDto dto,
        CancellationToken cancellationToken = default)
    {
        return documentUserRepository.GetAsync(new DocumentUserFilter
        {
            DocumentIds = [dto.DocumentId],
            UserIds = dto.UserIds
        }, cancellationToken);
    }

    public async Task UpdateDocumentContentAsync(
        UpdateDocumentContentDto dto,
        CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentOrThrowExceptionAsync(dto.DocumentId, cancellationToken);

        var documentUsers = await documentUserRepository.GetAsync(new DocumentUserFilter
        {
            DocumentIds = [document.Id],
            UserIds = [dto.UserId]
        }, cancellationToken);

        var currentUser = documentUsers.Single();

        if (currentUser.AccessLevel is not AccessLevel.Writer)
        {
            logger.LogError($"User с id={currentUser.UserId} попытался изменить файл, в котором у него доступ AccessLevel={currentUser.AccessLevel.ToString()}");
            throw new ExceptionWithStatusCode(
                "У вас недостаточно прав для выполнения этой операции",
                HttpStatusCode.Forbidden);
        }

        await documentRepository.UpdateAsync(document.Id,
            new UpdateDocumentDto(document.OwnerId, document.Title, dto.NewContent),
            cancellationToken);
    }

    public Task DeleteDocumentAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        return documentRepository.DeleteAsync(ids, cancellationToken);
    }

    private async Task<Document> GetDocumentOrThrowExceptionAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetAsync(id, cancellationToken);

        if (document is null)
        {
            logger.LogError($"Документ с идентификатором {id} не найден");
            throw new ExceptionWithStatusCode(
                $"Документ с идентификатором {id} не найден",
                HttpStatusCode.NotFound);
        }

        return document;
    }
}