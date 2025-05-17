using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Exceptions;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;
using CreateDocumentDto = CorporateSystem.SharedDocs.Services.Dtos.CreateDocumentDto;

namespace CorporateSystem.SharedDocs.Services.Services.Implementations;

internal class DocumentService(
    ILogger<DocumentService> logger,
    IDocumentRepository documentRepository,
    IDocumentUserRepository documentUserRepository,
    IAuthApiService authApiService,
    IDocumentCompositeRepository documentCompositeRepository,
    IDocumentChangeLogService documentChangeLogService) : IDocumentService
{
    public async Task<Document> GetDocumentAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetAsync(documentId, cancellationToken);

        if (document is null)
        {
            logger.LogError($"{nameof(GetDocumentAsync)}: document with id={documentId} not found");
            throw new FileNotFoundException("Документ не был найден");
        }

        return document;
    }

    public async Task<int> CreateDocumentAsync(CreateDocumentDto dto, CancellationToken cancellationToken = default)
    {
        var ids = await documentRepository.CreateAsync(
            [new Infrastructure.Dtos.CreateDocumentDto(dto.OwnerId, dto.Title, dto.Content)],
            cancellationToken);
        
        return ids.Single();
    }

    public async Task AddUsersToDocumentAsync(AddUserToDocumentDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation($"{nameof(AddUsersToDocumentAsync)}: dto.UserInfos.Count={dto.UserInfos.Length}");
        var document = await GetDocumentOrThrowExceptionAsync(dto.DocumentId, cancellationToken);
        logger.LogInformation($"{nameof(AddUsersToDocumentAsync)}: document.Id={document.Id}");

        var currentDocumentUsers = await documentUserRepository.GetAsync(new DocumentUserFilter
        {
            DocumentIds = [document.Id]
        }, cancellationToken);
        
        var currentDocumentsUsersList = currentDocumentUsers.ToList();
        
        var currentDocumentUserIds = currentDocumentsUsersList
            .Select(documentUser => documentUser.UserId)
            .ToList();

        logger.LogInformation($"{nameof(AddUsersToDocumentAsync)}: currentDocumentUserIds.Count={currentDocumentUserIds.Count}");
        
        var userIdsWhichNeedAdded = new List<DocumentUserInfo>();
        
        foreach (var userInfo in dto.UserInfos)
        {
            if (currentDocumentUserIds.Contains(userInfo.UserId))
            {
                logger.LogError($"{nameof(AddUsersToDocumentAsync)} Попытка добавить пользователя с идентификатором {userInfo.UserId}, который уже был добавлен ранее к текущему документу");
                throw new UserAlreadyExistException("Попытка добавить уже существующего пользователя");
            }
            
            userIdsWhichNeedAdded.Add(userInfo);
        }
        
        logger.LogInformation($"{nameof(AddUsersToDocumentAsync)}: {userIdsWhichNeedAdded.Count} пользователей будет добавлено к документу");
        
        await documentUserRepository
            .CreateAsync(
                userIdsWhichNeedAdded
                    .Select(userInfo => new CreateDocumentUserDto(dto.DocumentId, userInfo.UserId, userInfo.AccessLevel))
                    .ToArray(),
                cancellationToken);
        
        logger.LogInformation($"{nameof(AddUsersToDocumentAsync)}: Completed");
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

    public Task<IEnumerable<DocumentInfo>> GetCurrentUserDocuments(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return documentCompositeRepository.GetAsync(userId, cancellationToken);
    }
    
    public async Task<IEnumerable<UserInfo>> GetUsersOfCurrentDocument(
        int documentId,
        CancellationToken cancellationToken = default)
    {
        var currentDocument = await GetDocumentOrThrowExceptionAsync(documentId, cancellationToken);

        var usersCurrentDocument = await documentUserRepository.GetAsync(new DocumentUserFilter
        {
            DocumentIds = [currentDocument.Id]
        }, cancellationToken);

        var usersIds = usersCurrentDocument
            .Select(user => user.UserId)
            .ToArray();

        var userEmails = await authApiService.GetUserEmailsByIdsAsync(usersIds, cancellationToken: cancellationToken);

        return userEmails.Select(email => new UserInfo(email));
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
            logger.LogError(
                $"{nameof(UpdateDocumentContentAsync)}: User с id={currentUser.UserId} попытался изменить файл (document id={document.Id}), в котором у него доступ AccessLevel={currentUser.AccessLevel.ToString()}");
            throw new InsufficientPermissionsException("У вас недостаточно прав для выполнения этой операции");
        }

        await documentRepository.UpdateAsync(
            document.Id,
            new UpdateDocumentDto(document.Title, dto.NewContent),
            cancellationToken);
        
        await documentChangeLogService.AddChangeLogAsync(new ChangeLog
        {
            DocumentId = document.Id,
            UserId = dto.UserId,
            Changes = dto.NewContent,
            Line = dto.Line
        }, cancellationToken);
    }

    public Task DeleteUsersFromCurrentDocumentAsync(
        DeleteUserFromDocumentDto dto,
        CancellationToken cancellationToken = default)
    {
        return documentUserRepository.DeleteAsync(new DocumentUserFilter
        {
            DocumentIds = [dto.DocumentId],
            UserIds = [dto.UserId]
        }, cancellationToken);
    }

    public Task DeleteDocumentAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        return documentRepository.DeleteAsync(new DocumentFilter
        {
            Ids = ids
        }, cancellationToken);
    }

    private async Task<Document> GetDocumentOrThrowExceptionAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetAsync(id, cancellationToken);

        if (document is null)
        {
            logger.LogError($"{nameof(GetDocumentOrThrowExceptionAsync)}: Документ с идентификатором {id} не найден");
            throw new FileNotFoundException($"Документ с идентификатором {id} не найден");
        }

        return document;
    }
}