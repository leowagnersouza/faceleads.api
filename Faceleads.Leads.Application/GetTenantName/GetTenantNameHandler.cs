using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.GetTenantName;

public sealed class GetTenantNameHandler
{
    private readonly ITenantRepository _repo;

    public GetTenantNameHandler(ITenantRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<string?>> HandleAsync(GetTenantNameQuery query, CancellationToken cancellationToken = default)
    {
        var tenant = await _repo.GetByIdAsync(query.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null) return Result<string?>.Ok(null);

        return Result<string?>.Ok(tenant.Nome);
    }
}
