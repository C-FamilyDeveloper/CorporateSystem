using CorporateSystem.SharedDocs.Infrastructure.Dtos;
using CorporateSystem.SharedDocs.Infrastructure.Extensions;
using CorporateSystem.SharedDocs.Infrastructure.Options;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Filters;
using CorporateSystem.SharedDocs.Infrastructure.Repositories.Interfaces;
using Dapper;
using Microsoft.Extensions.Options;

namespace CorporateSystem.SharedDocs.Infrastructure.Repositories.Implementations;

internal class DocumentCompositeRepository(IOptions<PostgresOptions> options) 
    : PostgreRepository(options.Value), IDocumentCompositeRepository
{
    protected override string TableName { get; }

    public async Task<IEnumerable<DocumentInfo>> GetAsync(
        DocumentInfoFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var sqlQuery = @"
            select d.id as Id
                 , d.title as Title
              from documents d
              join document_users du on du.document_id = d.id";

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

        var documents = await connection.QueryAsync<DocumentInfo>(command);
        
        transaction.Complete();

        return documents;
    }

    public async Task<IEnumerable<DocumentInfo>> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        var sqlQuery = @"
            select d.id as Id
                 , d.title as Title
                 , case when d.owner_id = @UserId then true else false end as IsOwner
              from documents d
              join document_users du on du.document_id = d.id
             where du.user_id = @UserId";
        
        var command = new CommandDefinition(
            sqlQuery,
            new
            {
                UserId = userId
            },
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: cancellationToken);

        using var transaction = CreateTransactionScope();
        await using var connection = await GetAndOpenConnectionAsync(cancellationToken);

        var documents = await connection.QueryAsync<DocumentInfo>(command);
        
        transaction.Complete();

        return documents;
    }

    private DynamicParameters GetDynamicParametersForFilter(DocumentInfoFilter? filter, out List<string> conditions)
    {
        var @params = new DynamicParameters();

        conditions = new List<string>();

        if (filter is null)
        {
            return @params;
        }

        if (filter.OwnerIds.IsNotNullAndNotEmpty())
        {
            conditions.Add("d.owner_id = ANY(@OwnerIds)");
            @params.Add("OwnerIds", filter.OwnerIds);
        }

        if (filter.FollowerIds.IsNotNullAndNotEmpty())
        {
            conditions.Add("du.user_id = ANY(@FollowersIds)");
            @params.Add("FollowersIds", filter.FollowerIds);
        }

        return @params;
    }
}