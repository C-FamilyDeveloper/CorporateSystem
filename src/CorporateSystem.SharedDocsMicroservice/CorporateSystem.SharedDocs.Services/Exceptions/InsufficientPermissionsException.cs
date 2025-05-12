namespace CorporateSystem.SharedDocs.Services.Exceptions;

public class InsufficientPermissionsException : Exception
{
    public InsufficientPermissionsException(string message) : base(message)
    {
    }
}