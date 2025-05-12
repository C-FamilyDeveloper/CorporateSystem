using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using CorporateSystem.SharedDocs.Services.Dtos;
using CorporateSystem.SharedDocs.Services.Services.Interfaces;

namespace CorporateSystem.SharedDocs.Services.Services.Implementations;

internal class DocumentChangeLogService(IDocumentChangeLogRepository documentChangeLogRepository) 
    : IDocumentChangeLogService
{
    public async Task AddChangeLogAsync(ChangeLog changeLog, CancellationToken cancellationToken = default)
    {
        var documentLogs = await documentChangeLogRepository.GetAsync(new DocumentChangeLogFilter
        {
            DocumentIds = [changeLog.DocumentId]
        }, cancellationToken);

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
            
            }
            else
            {
                await CreateChangeLogAsync(changeLog, cancellationToken);
            }   
        }
    }

    private bool IsLogNeedChange(DocumentChangeLog documentChangeLog, ChangeLog changeLog)
    {
        if (documentChangeLog.UserId != changeLog.UserId)
        {
            return false;
        }

        var linesDifferent = documentChangeLog.StartLine - changeLog.StartLine;
        
        if (Math.Abs(linesDifferent) > 1)
        {
            return false;
        }

        if (linesDifferent == 0)
        {
            
        }
    }

    private Task CreateChangeLogAsync(ChangeLog changeLog, CancellationToken cancellationToken)
    {
        return documentChangeLogRepository.CreateAsync([
            new CreateDocumentChangeLog
            {
                DocumentId = changeLog.DocumentId,
                StartLine = changeLog.StartLine,
                UserId = changeLog.UserId,
                Changes = changeLog.Changes,
                StartColumn = changeLog.StartColumn
            }
        ], cancellationToken);
    }
}