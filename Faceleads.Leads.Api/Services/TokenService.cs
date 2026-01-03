using Faceleads.Leads.Domain;
using Faceleads.Leads.Infrastructure;
using Faceleads.Leads.Application.Common;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Faceleads.Leads.Api.Services;

public sealed class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenRepository _refreshRepo;

    public TokenService(IConfiguration configuration, IRefreshTokenRepository refreshRepo)
    {
        _configuration = configuration;
        _refreshRepo = refreshRepo;
    }

    public async Task<Result<(string accessToken, string refreshToken)>> IssueTokensAsync(string username, CancellationToken cancellationToken = default)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var jwtIssuer = jwtSettings["Issuer"]!;
        var jwtAudience = jwtSettings["Audience"]!;
        var jwtKey = jwtSettings["Key"]!;

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken(refreshTokenString, username, DateTime.UtcNow.AddDays(30));
        await _refreshRepo.AddAsync(refreshToken, cancellationToken).ConfigureAwait(false);

        return Result<(string, string)>.Ok((accessToken, refreshTokenString));
    }

    public async Task<Result<(string accessToken, string refreshToken)>> RotateRefreshTokenAsync(RefreshToken existing, CancellationToken cancellationToken = default)
    {
        existing.Revoke();
        await _refreshRepo.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);

        var newRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken(newRefresh, existing.Username, DateTime.UtcNow.AddDays(30));
        await _refreshRepo.AddAsync(refreshToken, cancellationToken).ConfigureAwait(false);

        var jwtSettings = _configuration.GetSection("Jwt");
        var jwtIssuer = jwtSettings["Issuer"]!;
        var jwtAudience = jwtSettings["Audience"]!;
        var jwtKey = jwtSettings["Key"]!;

        var claims = new[] { new Claim(ClaimTypes.Name, existing.Username) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return Result<(string, string)>.Ok((accessToken, newRefresh));
    }

    public async Task<Result<(string accessToken, string refreshToken)>> RefreshWithTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var existing = await _refreshRepo.GetByTokenAsync(refreshToken, cancellationToken).ConfigureAwait(false);

        if (existing is null || !existing.IsActive())
        {
            return Result<(string, string)>.Fail("REFRESH_INVALID", "Refresh token is invalid or expired");
        }

        return await RotateRefreshTokenAsync(existing, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var existing = await _refreshRepo.GetByTokenAsync(refreshToken, cancellationToken).ConfigureAwait(false);

        if (existing is null)
        {
            return Result.Ok();
        }

        existing.Revoke();
        await _refreshRepo.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);

        return Result.Ok();
    }
}
