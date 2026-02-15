using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Api.Adapters;

public sealed class TokenServiceAdapter : Faceleads.Leads.Application.Services.ITokenService
{
    private readonly Faceleads.Leads.Api.Services.ITokenService _inner;

    public TokenServiceAdapter(Faceleads.Leads.Api.Services.ITokenService inner)
    {
        _inner = inner;
    }

    public Task<Result<(string accessToken, string refreshToken)>> IssueTokensAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        return _inner.IssueTokensAsync(usuario, cancellationToken);
    }

    public Task<Result<(string accessToken, string refreshToken)>> RotateRefreshTokenAsync(RefreshToken existing, CancellationToken cancellationToken = default)
    {
        return _inner.RotateRefreshTokenAsync(existing, cancellationToken);
    }

    public Task<Result<(string accessToken, string refreshToken)>> RefreshWithTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return _inner.RefreshWithTokenAsync(refreshToken, cancellationToken);
    }

    public Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return _inner.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
    }
}
