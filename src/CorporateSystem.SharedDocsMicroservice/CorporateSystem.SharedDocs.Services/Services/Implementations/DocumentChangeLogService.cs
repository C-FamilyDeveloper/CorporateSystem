using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;

namespace CorporateSystem.SharedDocs.Services.Services.Implementations;

internal class DocumentChangeLogService(
    IDocumentChangeLogRepository documentChangeLogRepository,
    IAuthApiService authApiService) 
    : IDocumentChangeLogService
{
    public async Task AddChangeLogAsync(ChangeLog changeLog, CancellationToken cancellationToken = default)
    {
        var documentLogs = await GetDocumentChangeLogs(changeLog.DocumentId, cancellationToken);

        var documentLogsArray = documentLogs
            .OrderByDescending(documentLog => documentLog.ChangedAt)
            .ToArray();

        if (!documentLogsArray.Any())
        {
            await CreateChangeLogAsync(changeLog, cancellationToken);
        }
        else
        {
            var documentLog = documentLogsArray.First();

            if (IsLogNeedChange(documentLog, changeLog))
            {
                await documentChangeLogRepository
                    .UpdateAsync(documentLog.Id, new UpdateDocumentChangeLogDto(changeLog.Changes), cancellationToken);
            }
            else
            {
                await CreateChangeLogAsync(changeLog, cancellationToken);
            }   
        }
    }

    public async Task<DocumentChangeLogDto[]> GetChangeLogsAsync(
        int documentId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var documentLogs = await GetDocumentChangeLogs(documentId, cancellationToken);

        // todo: запилить пагинацию
        var documentLogsArray = documentLogs
            .OrderByDescending(log => log.ChangedAt)
            .Take(50)
            .ToArray();

        var result = new DocumentChangeLogDto[documentLogsArray.Length];

        for (var i = 0; i < documentLogsArray.Length; i++)
        {
            var documentLog = documentLogsArray[i];
            var authResponse = await authApiService.GetUserEmailsByIdsAsync(
                [documentLog.UserId],
                token,
                cancellationToken);
            
            var userEmail = authResponse.Single();

            result[i] = new DocumentChangeLogDto
            {
                Changes = documentLog.Changes,
                DocumentId = documentLog.DocumentId,
                Id = documentLog.Id,
                UserEmail = userEmail,
                ChangedAt = documentLog.ChangedAt
            };
        }

        return result;
    }

    private bool IsLogNeedChange(DocumentChangeLog documentChangeLog, ChangeLog changeLog)
    {
        if (documentChangeLog.UserId != changeLog.UserId || 
            documentChangeLog.Line != changeLog.Line)
        {
            return false;
        }

        return true;
    }

    private Task CreateChangeLogAsync(ChangeLog changeLog, CancellationToken cancellationToken)
    {
        return documentChangeLogRepository.CreateAsync([
            new CreateDocumentChangeLogDto
            {
                DocumentId = changeLog.DocumentId,
                Line = changeLog.Line,
                UserId = changeLog.UserId,
                Changes = changeLog.Changes
            }
        ], cancellationToken);
    }

    private Task<IEnumerable<DocumentChangeLog>> GetDocumentChangeLogs(
        int documentId,
        CancellationToken cancellationToken)
    {
        return documentChangeLogRepository.GetAsync(new DocumentChangeLogFilter
        {
            DocumentIds = [documentId]
        }, cancellationToken);
    }
}