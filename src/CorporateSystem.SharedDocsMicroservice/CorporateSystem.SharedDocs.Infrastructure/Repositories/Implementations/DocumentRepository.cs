using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Domain.Enums;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Extensions;
using CorporateSystem.SharedDocs.Infrastructure.Options;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using Dapper;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("CorporateSystem.SharedDocs.Tests")]
namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Implementations;

internal class DocumentRepository(IOptions<PostgresOptions> options) 
    : PostgreRepository(options.Value), IDocumentRepository
{
    protected override string TableName { get; } = "documents";

    public async Task<Document?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var sqlQuery = @$" select id as Id
                 , owner_id as OwnerId
                 , title as Title
                 , content as Content
                 , modified_at as ModifiedAt
                 , created_at as CreatedAt
              from {TableName}
             where id = @Id";
        var command = new CommandDefinition(
            sqlQuery,
            new
            {
                Id = id
            },
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);
        
        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        
        var result = await connection.QueryFirstOrDefaultAsync<Document>(command);
        
        transaction.Complete();
        return result;
    }

    public async Task<IEnumerable<Document>> GetAsync(
        DocumentFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var sqlQuery = @$"
            select id as Id
                 , owner_id as OwnerId
                 , title as Title
                 , content as Content
                 , modified_at as ModifiedAt
                 , created_at as CreatedAt
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
        var result = await connection.QueryAsync<Document>(command);
        
        transaction.Complete();
        
        return result;
    }

    public async Task<int[]> CreateAsync(CreateDocumentDto[] dtos, CancellationToken cancellationToken = default)
    {
        var sqlQueryForDocument = @$"
            insert into {TableName} (owner_id, title, content, created_at)
            select UNNEST(@OwnerIds)
                 , UNNEST(@Titles)
                 , UNNEST(@Contents)
                 , @CreatedAt
         returning id";
        
        var sqlQueryForDocumentUsers = @$"
            insert into document_users (document_id, user_id, access_level)
            select UNNEST(@DocumentIds), UNNEST(@UserIds), UNNEST(@AccessLevels)
         returning id";
        
        var commandForDocument = new CommandDefinition(
            sqlQueryForDocument,
            new
            {
                OwnerIds = dtos.Select(dto => dto.OwnerId).ToArray(),
                Titles = dtos.Select(dto => dto.Title).ToArray(),
                Contents = dtos.Select(dto => dto.Content).ToArray(),
                CreatedAt = DateTimeOffset.UtcNow
            },
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);
        
        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);

        var ids = await connection.QueryAsync<int>(commandForDocument);

        var idsArray = ids.ToArray();
        
        var commandForDocumentUser = new CommandDefinition(
            sqlQueryForDocumentUsers,
            new
            {
                DocumentIds = idsArray,
                UserIds = dtos.Select(dto => dto.OwnerId).ToArray(),
                AccessLevels = Enumerable
                    .Repeat((int)AccessLevel.Writer, idsArray.Length)
                    .ToArray()
            });

        await connection.QueryAsync<int>(commandForDocumentUser);
        
        transaction.Complete();
        
        return idsArray;
    }

    public async Task UpdateAsync(
        int id,
        UpdateDocumentDto updatedDocumentDto,
        CancellationToken cancellationToken = default)
    {
        var sqlQuery = @$"
            update {TableName}
               set title = @Title
                 , content = @Content
                 , modified_at = @ModifiedAt
             where id = @Id";
        
        var command = new CommandDefinition(
            sqlQuery,
            new
            {
                Id = id,
                Title = updatedDocumentDto.Title,
                Content = updatedDocumentDto.Content,
                ModifiedAt = DateTimeOffset.UtcNow
            });
        
        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(command);
        
        transaction.Complete();
    }

    public async Task DeleteAsync(DocumentFilter? filter = null, CancellationToken cancellationToken = default)
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

    private DynamicParameters GetDynamicParametersForFilter(DocumentFilter? filter, out List<string> conditions)
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

        if (filter.ModifiedAt.IsNotNullAndNotEmpty())
        {
            conditions.Add("modified_at = ANY(@ModifiedAt)");
            @params.Add("ModifiedAt", filter.ModifiedAt);
        }

        if (filter.Contents.IsNotNullAndNotEmpty())
        {
            conditions.Add("content = ANY(@Contents)");
            @params.Add("Contents", filter.Contents);
        }

        if (filter.Titles.IsNotNullAndNotEmpty())
        {
            conditions.Add("title = ANY(@Titles)");
            @params.Add("Titles", filter.Titles);
        }

        if (filter.OwnerIds.IsNotNullAndNotEmpty())
        {
            conditions.Add("owner_id = ANY(@OwnerIds)");
            @params.Add("OwnerIds", filter.OwnerIds);
        }

        if (filter.CreatedAt.IsNotNullAndNotEmpty())
        {
            conditions.Add("created_at = ANY(@CreatedAt)");
            @params.Add("CreatedAt", filter.CreatedAt);
        }

        return @params;
    }
}

