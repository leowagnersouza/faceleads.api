namespace Faceleads.Leads.Application.Common;

public interface ICurrentTenantService
{
    Guid TenantId { get; }
}
