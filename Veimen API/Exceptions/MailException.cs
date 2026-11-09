namespace Veimen_API.Exceptions;

public class MailException : Exception
{
    public int StatusCode { get; }

    public MailException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
