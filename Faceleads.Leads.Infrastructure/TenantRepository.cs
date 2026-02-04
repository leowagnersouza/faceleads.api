using Faceleads.Leads.Domain;
using Microsoft.EntityFrameworkCore;

namespace Faceleads.Leads.Infrastructure;

public sealed class TenantRepository : ITenantRepository
{
    private readonly LeadsDbContext _db;

    public TenantRepository(LeadsDbContext db)
    {
        _db = db;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Tenants.AsNoTracking().SingleOrDefaultAsync(t => t.Id == id, cancellationToken).ConfigureAwait(false);
    }
}
