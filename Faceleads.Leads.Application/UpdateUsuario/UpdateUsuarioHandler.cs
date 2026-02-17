using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Application.Repositories;
using Faceleads.Leads.Domain;
using Faceleads.Leads.Application.Services;

namespace Faceleads.Leads.Application.UpdateUsuario;

public sealed class UpdateUsuarioHandler
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public UpdateUsuarioHandler(IUsuarioRepository usuarioRepo, IPasswordHasher<Usuario> passwordHasher)
    {
        _usuarioRepo = usuarioRepo;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> HandleAsync(UpdateUsuarioCommand cmd, CancellationToken cancellationToken = default)
    {
        if (cmd.Id == Guid.Empty) return Result.Fail(Errors.Generic);

        var usuario = await _usuarioRepo.GetByIdAsync(cmd.Id, cancellationToken).ConfigureAwait(false);
        if (usuario is null) return Result.Fail(Errors.Generic);

        // Apply partial updates: only change fields that are not null
        if (cmd.NomeUsuario is not null)
        {
            usuario.AtualizarContato(cmd.NomeUsuario, usuario.Email);
        }

        if (cmd.Email is not null)
        {
            usuario.AtualizarContato(usuario.NomeUsuario, cmd.Email);
        }

        if (cmd.ConsultorId.HasValue)
        {
            var prop = typeof(Usuario).GetProperty("ConsultorId", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (prop is not null && prop.CanWrite)
            {
                prop.SetValue(usuario, cmd.ConsultorId.Value);
            }
        }

        if (cmd.Password is not null)
        {
            if (cmd.Password.Length < 6) 
                return Result.Fail(Errors.UsuarioSenhaCurta);

            // Hash password using injected hasher
            var hashed = _passwordHasher.HashPassword(usuario, cmd.Password);
            usuario.SetSenhaHash(hashed);
        }

        if (cmd.Ativo.HasValue)
        {
            if (cmd.Ativo.Value) usuario.Ativar(); else usuario.Desativar();
        }

        await _usuarioRepo.UpdateAsync(usuario, cancellationToken).ConfigureAwait(false);
        return Result.Ok();
    }
}
