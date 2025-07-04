using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Extensions;
using CorporateSystem.SharedDocs.Infrastructure.Options;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using Dapper;
using Microsoft.Extensions.Options;

namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Implementations;

internal class DocumentChangeLogRepository(IOptions<PostgresOptions> options) 
    : PostgreRepository(options.Value), IDocumentChangeLogRepository
{
    protected override string TableName { get; } = "document_change_logs";
    
    public async Task<IEnumerable<DocumentChangeLog>> GetAsync(
        DocumentChangeLogFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var sqlQuery = @$"
            select id as Id
                 , user_id as UserId
                 , document_id as DocumentId
                 , changed_at as ChangedAt
                 , changes as Changes
                 , line as Line
              from {TableName}";
        
        var @params = GetDynamicParametersForFilter(filter, out var conditions);

        if (conditions.Any())
        {
            sqlQuery += $" where {string.Join(" and ", conditions)} ";
        }
        
        var command = new CommandDefinition(
            sqlQuery,
            @params,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);
        
        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync<DocumentChangeLog>(command);
        
        transaction.Complete();
        
        return result;
    }

    public async Task<int[]> CreateAsync(CreateDocumentChangeLogDto[] dtos, CancellationToken cancellationToken = default)
    {
        var sqlQuery = @$"
            insert into {TableName} (document_id, user_id, changed_at, changes, line)
            select UNNEST(@DocumentIds), UNNEST(@UserIds), @ChangedAt, UNNEST(@Changes), UNNEST(@Lines)
         returning id";
        
        var command = new CommandDefinition(
            sqlQuery, new
            {
                DocumentIds = dtos.Select(dto => dto.DocumentId).ToArray(),
                UserIds = dtos.Select(dto => dto.UserId).ToArray(),
                Changes = dtos.Select(dto => dto.Changes).ToArray(),
                ChangedAt = DateTimeOffset.UtcNow,
                Lines = dtos.Select(dto => dto.Line).ToArray()
            },
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);
        
        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        
        var ids =  await connection.QueryAsync<int>(command);
        transaction.Complete();
        
        return ids.ToArray();
    }

    public async Task UpdateAsync(int id, UpdateDocumentChangeLogDto dto, CancellationToken cancellationToken = default)
    {
        var sqlQuery = @$"
            update {TableName}
               set changes = @Changes
             where id = @Id";
        
        var command = new CommandDefinition(
            sqlQuery,
            new
            {
                Id = id,
                Changes = dto.Changes
            },
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);
        
        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(command);
        
        transaction.Complete();
    }

    public async Task DeleteAsync(DocumentChangeLogFilter? filter = null, CancellationToken cancellationToken = default)
    {
        var sqlQuery = $"delete from {TableName}";

        var @params = GetDynamicParametersForFilter(filter, out var conditions);
        
        if (conditions.Any())
        {
            sqlQuery += $" where {string.Join(" and ", conditions)} ";
        }
        
        var command = new CommandDefinition(
            sqlQuery,
            @params,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);
        
        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(command);
        transaction.Complete();
    }
    
    private DynamicParameters GetDynamicParametersForFilter(DocumentChangeLogFilter? filter, out List<string> conditions)
    {
        var @params = new DynamicParameters();

        conditions = new List<string>();

        if (filter is null)
        {
            return @params;
        }
        
        if (filter.Ids.IsNotNullAndNotEmpty())
        {
            conditions.Add("id = ANY(@Ids)");
            @params.Add("Ids", filter.Ids);
        }

        if (filter.DocumentIds.IsNotNullAndNotEmpty())
        {
            conditions.Add("document_id = ANY(@DocumentIds)");
            @params.Add("DocumentIds", filter.DocumentIds);
        }

        if (filter.UserIds.IsNotNullAndNotEmpty())
        {
            conditions.Add("user_id = ANY(@UserIds)");
            @params.Add("UserIds", filter.UserIds);
        }

        return @params;
    }
}