using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Application.Repositories;
using Faceleads.Leads.Domain;
using Faceleads.Leads.Application.Services;

namespace Faceleads.Leads.Application.CreateUsuario;

public sealed class CreateUsuarioHandler
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly ICurrentTenantService _tenantService;

    public CreateUsuarioHandler(IUsuarioRepository usuarioRepo, IPasswordHasher<Usuario> passwordHasher, ICurrentTenantService tenantService)
    {
        _usuarioRepo = usuarioRepo;
        _passwordHasher = passwordHasher;
        _tenantService = tenantService;
    }

    public async Task<Result<Usuario>> HandleAsync(CreateUsuarioCommand cmd, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.NomeUsuario)) return Result<Usuario>.Fail(Errors.UsuarioNomeObrigatorio);
        if (string.IsNullOrWhiteSpace(cmd.Email)) return Result<Usuario>.Fail(Errors.UsuarioEmailObrigatorio);

        var tenantId = _tenantService.TenantId;

        // Ensure email and username uniqueness within tenant
        var normalizedEmail = cmd.Email.ToUpperInvariant();
        var existingByEmail = await _usuarioRepo.GetByNormalizedEmailAsync(tenantId, normalizedEmail, cancellationToken).ConfigureAwait(false);
        if (existingByEmail is not null) return Result<Usuario>.Fail(Errors.UsuarioJaExiste);

        var normalizedUsername = cmd.NomeUsuario.ToUpperInvariant();
        var existingByUsername = await _usuarioRepo.GetByNormalizedUsernameAsync(tenantId, normalizedUsername, cancellationToken).ConfigureAwait(false);
        if (existingByUsername is not null) return Result<Usuario>.Fail(Errors.UsuarioJaExiste);

        // Validate password presence/length
        if (string.IsNullOrWhiteSpace(cmd.Password)) return Result<Usuario>.Fail(Errors.UsuarioSenhaObrigatoria);
        if (cmd.Password.Length < 6) return Result<Usuario>.Fail(Errors.UsuarioSenhaCurta);

        // Create user instance first so hasher can use user context if needed
        var usuario = new Usuario(tenantId, cmd.NomeUsuario, cmd.Email, string.Empty);

        // Hash password using the hasher implementation (which expects the user instance)
        var hashed = _passwordHasher.HashPassword(usuario, cmd.Password);
        usuario.SetSenhaHash(hashed);

        if (cmd.ConsultorId.HasValue)
        {
            var prop = typeof(Usuario).GetProperty("ConsultorId", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (prop is not null && prop.CanWrite)
            {
                prop.SetValue(usuario, cmd.ConsultorId.Value);
            }
        }

        await _usuarioRepo.AddAsync(usuario, cancellationToken).ConfigureAwait(false);

        // Note: plainPassword should be communicated to the user (email/admin UI). Not returned here for security.
        return Result<Usuario>.Ok(usuario);
    }
}
