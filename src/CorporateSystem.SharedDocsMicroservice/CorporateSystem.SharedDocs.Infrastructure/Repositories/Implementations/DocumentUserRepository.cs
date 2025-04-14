using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Options;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using Dapper;
using Microsoft.Extensions.Options;

namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Implementations;

internal class DocumentUserRepository(IOptions<PostgreOptions> options) 
    : PostgreRepository(options.Value), IDocumentUserRepository
{
    protected override string TableName { get; } = "document_users";

    public async Task<DocumentUser?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        using var transaction = CreateTransactionScope();

        var sqlQuery = $"select * from {TableName} where id = @Id";
        var @params = new DynamicParameters();
        @params.Add("Id", id);
        
        var command = new CommandDefinition(
            sqlQuery,
            @params,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken:cancellationToken);

        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        
        return await connection.QueryFirstOrDefaultAsync<DocumentUser>(command);
    }

    public async Task<IEnumerable<DocumentUser>> GetAsync(
        DocumentUserFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        using var transaction = CreateTransactionScope();

        var sqlQuery = $"select * from {TableName}";
        var @params = new DynamicParameters();
        
        var conditions = new List<string>();
        if (filter is not null)
        {
            if (filter.Ids?.Length != 0)
            {
                conditions.Add("id = ANY(@Ids)");
                @params.Add("Ids", filter.Ids);
            }

            if (filter.AccessLevels?.Length != 0)
            {
                conditions.Add("access_level = ANY(@AccessLevels)");
                @params.Add("AccessLevels", filter.AccessLevels);
            }

            if (filter.DocumentIds?.Length != 0)
            {
                conditions.Add("document_id = ANY(@DocumentIds)");
                @params.Add("DocumentIds", filter.DocumentIds);
            }

            if (filter.UserIds?.Length != 0)
            {
                conditions.Add("user_id = ANY(@UserIds)");
                @params.Add("UserIds", filter.UserIds);
            }
        }

        if (conditions.Any())
        {
            sqlQuery += $" where {string.Join(" and ", conditions)} ";
        }
        
        var command = new CommandDefinition(
            sqlQuery,
            @params,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);
        
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        return await connection.QueryAsync<DocumentUser>(command);
    }

    public async Task<int[]> CreateAsync(CreateDocumentUserDto[] dtos, CancellationToken cancellationToken = default)
    {
        using var transaction = CreateTransactionScope();

        var sqlQuery = @$"
            insert into {TableName} (document_id, user_id, access_level)
            select document_id, user_id, access_level
              from UNNEST(@Dtos)
         returning id";

        var command = new CommandDefinition(
            sqlQuery, new
            {
                Dtos = dtos
            },
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);
        
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        
        var ids =  await connection.QueryAsync<int>(command);

        return ids.ToArray();
    }

    public async Task UpdateAsync(int id, UpdateDocumentUserDto dto, CancellationToken cancellationToken = default)
    {
        using var transaction = CreateTransactionScope();

        var sqlQuery = @$"
            update {TableName}
               set document_id = @DocumentId
                 , user_id = @UserId
                 , access_level = @AccessLevel
             where id = @Id";
        
        var command = new CommandDefinition(
            sqlQuery,
            new
            {
                Id = id,
                DocumentId = dto.DocumentId,
                UserId = dto.UserId,
                AccessLevel = dto.AccessLevel
            },
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);
        
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(command);
        
        transaction.Complete();
    }

    public async Task DeleteAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        using var transaction = CreateTransactionScope();

        var sqlQuery = $"delete * from {TableName} where id = ANY(@Ids)";
        var command = new CommandDefinition(
            sqlQuery,
            new
            {
                Ids = ids
            }, 
            commandTimeout: DefaultTimeoutInSeconds, 
            cancellationToken: cancellationToken);
        
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        
        await connection.ExecuteAsync(command);
        
        transaction.Complete();
    }
}