using Faceleads.Leads.Domain;
using Faceleads.Leads.Application.Common;

namespace Faceleads.Leads.Api.Services;

public interface ITokenService
{
    Task<Result<(string accessToken, string refreshToken)>> IssueTokensAsync(string username, CancellationToken cancellationToken = default);

    Task<Result<(string accessToken, string refreshToken)>> RotateRefreshTokenAsync(RefreshToken existing, CancellationToken cancellationToken = default);

    Task<Result<(string accessToken, string refreshToken)>> RefreshWithTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
