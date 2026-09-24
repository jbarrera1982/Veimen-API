namespace Veimen_API.Exceptions;

public class AuthException : Exception
{
    public int StatusCode { get; }

    public AuthException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}