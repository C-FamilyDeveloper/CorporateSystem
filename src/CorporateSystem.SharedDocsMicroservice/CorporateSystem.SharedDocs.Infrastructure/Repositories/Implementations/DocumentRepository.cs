using System.Runtime.CompilerServices;
using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Infrastructure.Dtos;
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
        var sqlQuery = $"select * from {TableName} where id = @Id";
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

            if (filter.ModifiedAt?.Length != 0)
            {
                conditions.Add("modified_at = ANY(@ModifiedAt)");
                @params.Add("ModifiedAt", filter.ModifiedAt);
            }

            if (filter.Contents?.Length != 0)
            {
                conditions.Add("content = ANY(@Contents)");
                @params.Add("Contents", filter.Contents);
            }

            if (filter.Titles?.Length != 0)
            {
                conditions.Add("titles = ANY(@Titles)");
                @params.Add("Titles", filter.Titles);
            }

            if (filter.OwnerIds?.Length != 0)
            {
                conditions.Add("owner_id = ANY(@OwnerIds)");
                @params.Add("OwnerIds", filter.OwnerIds);
            }

            if (filter.CreatedAt?.Length != 0)
            {
                conditions.Add("created_at = ANY(@CreatedAt)");
                @params.Add("CreatedAt", filter.CreatedAt);
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
        
        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync<Document>(command);
        
        transaction.Complete();
        
        return result;
    }

    public async Task<int[]> CreateAsync(CreateDocumentDto[] dtos, CancellationToken cancellationToken = default)
    {
        var sqlQuery = @$"
            insert into {TableName} (owner_id, title, content, created_at)
            select UNNEST(@OwnerIds)
                 , UNNEST(@Titles)
                 , UNNEST(@Contents)
                 , @CreatedAt
         returning id";
        
        var command = new CommandDefinition(
            sqlQuery,
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

        var ids = await connection.QueryAsync<int>(command);
        transaction.Complete();
        
        return ids.ToArray();
    }

    public async Task UpdateAsync(
        int id,
        UpdateDocumentDto updatedDocumentDto,
        CancellationToken cancellationToken = default)
    {
        var sqlQuery = @$"
            update {TableName}
               set owner_id = @OwnerId
                 , title = @Title
                 , content = @Content
                 , modified_at = @ModifiedAt
             where id = @Id";
        
        var command = new CommandDefinition(
            sqlQuery,
            new
            {
                Id = id,
                OwnerId = updatedDocumentDto.OwnerId,
                Title = updatedDocumentDto.Title,
                Content = updatedDocumentDto.Content,
                ModifiedAt = DateTimeOffset.UtcNow
            });
        
        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(command);
        
        transaction.Complete();
    }

    public async Task DeleteAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        var sqlQuery = $"delete from {TableName} where id = ANY(@Ids)";
        var command = new CommandDefinition(
            sqlQuery,
            new
            {
                Ids = ids
            },
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);
        
        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(command);
        transaction.Complete();
    }
}

