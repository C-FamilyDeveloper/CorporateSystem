using CorporateSystem.SharedDocs.Domain.Entities;
using CorporateSystem.SharedDocs.Tests.Helpers;

namespace CorporateSystem.SharedDocs.Tests.Builders;

internal class DocumentBuilder
{
    private int? _id = null;
    private string? _content = null;
    private int? _ownerId = null;
    private string? _title = null;
    private DateTimeOffset? _createdAt = null;
    private DateTimeOffset? _modifiedAt = null;

    public DocumentBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public DocumentBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public DocumentBuilder WithOwnerId(int ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    public DocumentBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public DocumentBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public DocumentBuilder WithModifiedAt(DateTimeOffset modifiedAt)
    {
        _modifiedAt = modifiedAt;
        return this;
    }
    
    public Document Build()
    {
        return new Document
        {
            Title = _title ?? StringHelper.GetUniqueString(),
            CreatedAt = _createdAt ?? DateTimeOffset.UtcNow,
            ModifiedAt = _modifiedAt ?? null,
            OwnerId = _ownerId ?? Int.GetUniqueNumber(),
            Content = _content ?? string.Empty,
            Id = _id ?? Int.GetUniqueNumber()
        };
    }   
}