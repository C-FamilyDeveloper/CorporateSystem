using CorporateSystem.SharedDocs.Domain.Enums;

namespace CorporateSystem.SharedDocs.Domain.Entities;

public class DocumentUser
{
    public int Id { get; init; }
    public int DocumentId { get; init; }
    public int UserId { get; init; }
    public AccessLevel AccessLevel { get; init; }
}