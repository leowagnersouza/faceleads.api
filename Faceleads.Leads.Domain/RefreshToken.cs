namespace Faceleads.Leads.Domain;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public string Username { get; private set; } = string.Empty;

    public DateTime ExpiresUtc { get; private set; }

    public DateTime CreatedUtc { get; private set; }

    public DateTime? RevokedUtc { get; private set; }

    private RefreshToken()
    {
        // EF Core
    }

    public RefreshToken(string token, string username, DateTime expiresUtc)
    {
        Id = Guid.NewGuid();
        Token = token;
        Username = username;
        ExpiresUtc = expiresUtc;
        CreatedUtc = DateTime.UtcNow;
    }

    public void Revoke()
    {
        if (RevokedUtc is not null)
        {
            return;
        }

        RevokedUtc = DateTime.UtcNow;
    }

    public bool IsActive() => RevokedUtc is null && DateTime.UtcNow < ExpiresUtc;
}

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
