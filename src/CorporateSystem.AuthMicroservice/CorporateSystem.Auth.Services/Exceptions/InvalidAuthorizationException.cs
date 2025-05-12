namespace CorporateSystem.Auth.Services.Exceptions;

public class InvalidAuthorizationException : Exception
{
    public InvalidAuthorizationException(string message) : base(message)
    {
    }
}