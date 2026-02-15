using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.Repositories;

public interface IRolePermissaoRepository
{
    Task AddAsync(RolePermissao rolePermissao, CancellationToken cancellationToken = default);

    Task<IEnumerable<Permissao>> GetPermissoesForRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
}
