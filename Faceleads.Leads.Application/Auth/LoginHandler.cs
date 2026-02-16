using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Application.Repositories;
using Faceleads.Leads.Domain;
using Faceleads.Leads.Application.Services;

namespace Faceleads.Leads.Application.Auth;

public sealed class LoginHandler
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly ITenantRepository _tenantRepo;
    private readonly ICurrentTenantService _currentTenantService;

    public LoginHandler(IUsuarioRepository usuarioRepo, ITokenService tokenService, IPasswordHasher<Usuario> passwordHasher, ITenantRepository tenantRepo, ICurrentTenantService currentTenantService)
    {
        _usuarioRepo = usuarioRepo;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _tenantRepo = tenantRepo;
        _currentTenantService = currentTenantService;
    }

    public async Task<Result<LoginResult>> HandleAsync(LoginCommand cmd, CancellationToken cancellationToken = default)
    {
        var normalized = cmd.Username.ToUpperInvariant();
        // Resolve tenant from current context and load user within tenant
        var tenantId = _currentTenantService.TenantId;
        var usuario = await _usuarioRepo.GetWithRolesByNormalizedUsernameAsync(tenantId, normalized, cancellationToken).ConfigureAwait(false);
        if (usuario is null && tenantId != Guid.Empty)
        {
            // Fallback to global tenant (some users may be stored without tenant scope)
            usuario = await _usuarioRepo.GetWithRolesByNormalizedUsernameAsync(Guid.Empty, normalized, cancellationToken).ConfigureAwait(false);
        }

        if (usuario is null)
        {
            return Result<LoginResult>.Fail(Errors.AuthInvalid);
        }

        var verified = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, cmd.Password);
        if (!verified)
        {
            // Try fallback to PBKDF2 hasher in case stored hashes were from the older hasher
            try
            {
                var fallback = new Faceleads.Leads.Application.Services.Pbkdf2PasswordHasher<Usuario>();
                if (!fallback.VerifyHashedPassword(usuario, usuario.SenhaHash, cmd.Password))
                {
                    return Result<LoginResult>.Fail(Errors.AuthInvalid);
                }
            }
            catch
            {
                return Result<LoginResult>.Fail(Errors.AuthInvalid);
            }
        }

        var issueResult = await _tokenService.IssueTokensAsync(usuario, cancellationToken).ConfigureAwait(false);
        if (!issueResult.Success)
        {
            return Result<LoginResult>.Fail(Errors.Generic);
        }

        var tuple = issueResult.Value!;
        string access = tuple.accessToken;
        string refresh = tuple.refreshToken;

        string? tenantName = null;
        if (usuario.TenantId != Guid.Empty)
        {
            var tenant = await _tenantRepo.GetByIdAsync(usuario.TenantId, cancellationToken).ConfigureAwait(false);
            tenantName = tenant?.Nome;
        }

        var result = new LoginResult
        {
            AccessToken = access,
            RefreshToken = refresh,
            TenantName = tenantName,
            Username = usuario.NomeUsuario
        };

        return Result<LoginResult>.Ok(result);
    }
}
