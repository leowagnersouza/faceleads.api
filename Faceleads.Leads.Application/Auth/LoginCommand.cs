namespace Faceleads.Leads.Application.Auth;

public sealed class LoginCommand
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
