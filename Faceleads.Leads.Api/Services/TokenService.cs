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

    public async Task<Result<(string accessToken, string refreshToken)>> IssueTokensAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var jwtIssuer = jwtSettings["Issuer"]!;
        var jwtAudience = jwtSettings["Audience"]!;
        var jwtKey = jwtSettings["Key"]!;

        // Build claims from the Usuario entity
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, usuario.NomeUsuario),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString())
        };

        if (usuario.TenantId != Guid.Empty)
        {
            claims.Add(new Claim("tenant_id", usuario.TenantId.ToString()));
        }

        // Add role claims if roles are loaded on the aggregate
        if (usuario.Roles is not null)
        {
            foreach (var ur in usuario.Roles)
            {
                if (ur.Role is not null && !string.IsNullOrEmpty(ur.Role.Nome))
                {
                    claims.Add(new Claim(ClaimTypes.Role, ur.Role.Nome));
                }
            }
        }

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
        var refreshToken = new RefreshToken(refreshTokenString, usuario.NomeUsuario, DateTime.UtcNow.AddDays(30));
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
        // Keep tenant claim from the existing token/user. For now use default tenant id.
        var defaultTenantId = "e7a1f3c2-9b4d-4f6a-8c12-3b9d2f0a6e5f";
        var claims = new[] { new Claim(ClaimTypes.Name, existing.Username), new Claim("tenant_id", defaultTenantId) };
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
            return Result<(string, string)>.Fail(Errors.RefreshInvalid);
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
