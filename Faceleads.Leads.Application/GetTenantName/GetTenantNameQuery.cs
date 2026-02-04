namespace Faceleads.Leads.Application.GetTenantName;

public sealed class GetTenantNameQuery
{
    public Guid TenantId { get; }

    public GetTenantNameQuery(Guid tenantId) => TenantId = tenantId;
}
