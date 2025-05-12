using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Extensions;
using CorporateSystem.SharedDocs.Infrastructure.Options;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using Dapper;
using Microsoft.Extensions.Options;

namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Implementations;

internal class DocumentUserRepository(IOptions<PostgresOptions> options) 
    : PostgreRepository(options.Value), IDocumentUserRepository
{
    protected override string TableName { get; } = "document_users";

    public async Task<DocumentUser?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var sqlQuery = @$"select id as Id
                 , access_level as AccessLevel
                 , document_id as DocumentId
                 , user_id as UserId
              from {TableName} 
             where id = @Id";
        var @params = new DynamicParameters();
        @params.Add("Id", id);
        
        var command = new CommandDefinition(
            sqlQuery,
            @params,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken:cancellationToken);

        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        
        var result = await connection.QueryFirstOrDefaultAsync<DocumentUser>(command);
        transaction.Complete();

        return result;
    }

    public async Task<IEnumerable<DocumentUser>> GetAsync(
        DocumentUserFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var sqlQuery = @$"
            select id as Id
                 , access_level as AccessLevel
                 , document_id as DocumentId
                 , user_id as UserId 
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
        
        var result = await connection.QueryAsync<DocumentUser>(command);
        
        transaction.Complete();
        return result;
    }

    public async Task<int[]> CreateAsync(CreateDocumentUserDto[] dtos, CancellationToken cancellationToken = default)
    {
        var sqlQuery = @$"
            insert into {TableName} (document_id, user_id, access_level)
            select UNNEST(@DocumentIds), UNNEST(@UserIds), UNNEST(@AccessLevels)
         returning id";

        var command = new CommandDefinition(
            sqlQuery, new
            {
                DocumentIds = dtos.Select(dto => dto.DocumentId).ToArray(),
                UserIds = dtos.Select(dto => dto.UserId).ToArray(),
                AccessLevels = dtos.Select(dto => (int)dto.AccessLevel).ToArray()
            },
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);
        
        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        
        var ids =  await connection.QueryAsync<int>(command);
        transaction.Complete();
        
        return ids.ToArray();
    }

    public async Task UpdateAsync(int id, UpdateDocumentUserDto dto, CancellationToken cancellationToken = default)
    {
        var sqlQuery = @$"
            update {TableName}
               set user_id = @UserId
                 , access_level = @AccessLevel
             where id = @Id";
        
        var command = new CommandDefinition(
            sqlQuery,
            new
            {
                Id = id,
                UserId = dto.UserId,
                AccessLevel = dto.AccessLevel
            },
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);
        
        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(command);
        
        transaction.Complete();
    }

    public async Task DeleteAsync(DocumentUserFilter? filter = null, CancellationToken cancellationToken = default)
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

    private DynamicParameters GetDynamicParametersForFilter(DocumentUserFilter? filter, out List<string> conditions)
    {
        var @params = new DynamicParameters();
        
        conditions = new List<string>();
        if (filter is not null)
        {
            if (filter.Ids.IsNotNullAndNotEmpty())
            {
                conditions.Add("id = ANY(@Ids)");
                @params.Add("Ids", filter.Ids);
            }

            if (filter.AccessLevels.IsNotNullAndNotEmpty())
            {
                conditions.Add("access_level = ANY(@AccessLevels)");
                @params.Add("AccessLevels", filter
                    .AccessLevels!
                    .Select(accessLevel => (int)accessLevel)
                    .ToArray());
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
        }

        return @params;
    } 
}