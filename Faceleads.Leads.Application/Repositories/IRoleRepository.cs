using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.Repositories;

public interface IRoleRepository
{
    Task AddAsync(Role role, CancellationToken cancellationToken = default);

    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Role?> GetByNormalizedNameAsync(Guid? tenantId, string normalizedName, CancellationToken cancellationToken = default);

    Task<IEnumerable<Role>> GetRolesForTenantAsync(Guid? tenantId, CancellationToken cancellationToken = default);
}
