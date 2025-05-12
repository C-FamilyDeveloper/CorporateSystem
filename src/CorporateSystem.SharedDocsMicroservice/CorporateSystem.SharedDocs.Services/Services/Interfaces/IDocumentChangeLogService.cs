using CorporateSystem.SharedDocs.Services.Dtos;

namespace CorporateSystem.SharedDocs.Services.Services.Interfaces;

public interface IDocumentChangeLogService
{
    Task AddChangeLogAsync(ChangeLog changeLog, CancellationToken cancellationToken = default);
}