namespace CorporateSystem.SharedDocs.Services.Exceptions;

public class UserAlreadyExistException : Exception
{
    public UserAlreadyExistException(string message) : base(message)
    {
    }
}