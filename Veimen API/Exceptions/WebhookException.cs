namespace Veimen_API.Exceptions;

public class WebhookException : Exception
{
    public int StatusCode { get; }

    public WebhookException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
