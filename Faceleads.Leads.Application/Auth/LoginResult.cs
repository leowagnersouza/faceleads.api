namespace Faceleads.Leads.Application.Auth;

public sealed class LoginResult
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public string? TenantName { get; init; }
    public string Username { get; init; } = string.Empty;
}
