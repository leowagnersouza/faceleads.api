using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Domain;
using Faceleads.Leads.Application.Repositories;

namespace Faceleads.Leads.Application.ListUsuarios;

public sealed class ListUsuariosHandler
{
    private readonly IUsuarioRepository _usuarioRepo;

    public ListUsuariosHandler(IUsuarioRepository usuarioRepo)
    {
        _usuarioRepo = usuarioRepo;
    }

    public async Task<Result<IEnumerable<Usuario>>> HandleAsync(ListUsuariosQuery query, CancellationToken cancellationToken = default)
    {
        // For now basic list via repository (should add paging/filters later)
        // Reuse existing repository via DB context
        var users = await _usuarioRepo.ListAsync(cancellationToken).ConfigureAwait(false);
        return Result<IEnumerable<Usuario>>.Ok(users);
    }
}
