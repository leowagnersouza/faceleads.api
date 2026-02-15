using Faceleads.Leads.Application.Repositories;
using Faceleads.Leads.Domain;
using Microsoft.EntityFrameworkCore;

namespace Faceleads.Leads.Infrastructure.Repositories;

public sealed class RolePermissaoRepository : IRolePermissaoRepository
{
    private readonly LeadsDbContext _db;

    public RolePermissaoRepository(LeadsDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(RolePermissao rolePermissao, CancellationToken cancellationToken = default)
    {
        await _db.Set<RolePermissao>().AddAsync(rolePermissao, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Permissao>> GetPermissoesForRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _db.RolesPermissoes
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permissao!)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
