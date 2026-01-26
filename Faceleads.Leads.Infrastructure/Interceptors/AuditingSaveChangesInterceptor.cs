using Faceleads.Leads.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

// Interceptor also assigns TenantId for new entities when available

namespace Faceleads.Leads.Infrastructure.Interceptors;

public sealed class AuditingSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditingSaveChangesInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditProperties(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateAuditProperties(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditProperties(DbContext? context)
    {
        if (context == null) return;

        var now = DateTime.UtcNow;
        var userId = _currentUserService.UserId ?? string.Empty;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                // If entity has a TenantId property, set it from the current tenant service (if available)
                var tenantProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "TenantId");
                if (tenantProp != null && tenantProp.CurrentValue is null)
                {
                    try
                    {
                        var tenantService = context as Microsoft.EntityFrameworkCore.Infrastructure.IInfrastructure<IServiceProvider>;
                        var sp = tenantService?.Instance;
                        var currentTenant = sp?.GetService(typeof(Faceleads.Leads.Application.Common.ICurrentTenantService)) as Faceleads.Leads.Application.Common.ICurrentTenantService;
                        if (tenantService != null)
                        {
                            tenantProp.CurrentValue = currentTenant?.TenantId;
                        }
                    }
                    catch
                    {
                        // ignore if tenant service not available at design-time or other contexts
                    }
                }

                if (entry.Property("CreatedOn") != null)
                {
                    entry.Property("CreatedOn").CurrentValue = now;
                }
                if (entry.Property("CreatedBy") != null)
                {
                    entry.Property("CreatedBy").CurrentValue = userId;
                }
            }

            if (entry.State == EntityState.Modified)
            {
                if (entry.Property("ModifiedOn") != null)
                {
                    entry.Property("ModifiedOn").CurrentValue = now;
                }
                if (entry.Property("ModifiedBy") != null)
                {
                    entry.Property("ModifiedBy").CurrentValue = userId;
                }
            }
        }
    }
}
