namespace Faceleads.Leads.Application.Common;

public sealed class Error
{
    public string Code { get; }

    public string Message { get; }

    public int StatusCode { get; }

    public Error(string code, string message, int statusCode = 400)
    {
        Code = code;
        Message = message;
        StatusCode = statusCode;
    }

    public override string ToString() => $"{Code}: {Message}";
}
