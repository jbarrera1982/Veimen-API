namespace Veimen_API.Exceptions;

public class OpenAiException : Exception
{
    public int StatusCode { get; }

    public OpenAiException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
