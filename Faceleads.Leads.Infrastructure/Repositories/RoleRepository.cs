using Faceleads.Leads.Application.Repositories;
using Faceleads.Leads.Domain;
using Microsoft.EntityFrameworkCore;

namespace Faceleads.Leads.Infrastructure.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly LeadsDbContext _db;

    public RoleRepository(LeadsDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _db.Set<Role>().AddAsync(role, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Roles.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Role?> GetByNormalizedNameAsync(Guid? tenantId, string normalizedName, CancellationToken cancellationToken = default)
    {
        return await _db.Roles.AsNoTracking().SingleOrDefaultAsync(r => r.TenantId == tenantId && r.NormalizedNome == normalizedName, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Role>> GetRolesForTenantAsync(Guid? tenantId, CancellationToken cancellationToken = default)
    {
        return await _db.Roles.AsNoTracking().Where(r => r.TenantId == tenantId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
