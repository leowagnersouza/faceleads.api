using Faceleads.Leads.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

// Interceptor also assigns TenantId for new entities when available

namespace Faceleads.Leads.Infrastructure.Interceptors;

public sealed class AuditingSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrentTenantService? _currentTenantService;

    public AuditingSaveChangesInterceptor(ICurrentUserService currentUserService, ICurrentTenantService? currentTenantService = null)
    {
        _currentUserService = currentUserService;
        _currentTenantService = currentTenantService;
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
        // Ensure EF has detected any changes so we can observe Modified states
        context.ChangeTracker.DetectChanges();

        var now = DateTime.UtcNow;
        var userId = _currentUserService.UserId ?? string.Empty;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                // If entity has a TenantId property, set it from the current tenant service (if available)
                var tenantProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "TenantId");
                if (tenantProp != null)
                {
                    // Treat Guid.Empty as not set for non-nullable TenantId columns
                    var current = tenantProp.CurrentValue;
                    var isEmptyGuid = current is Guid g && g == Guid.Empty;
                    var isNull = current is null;
                    if (isNull || isEmptyGuid)
                    {
                        try
                        {
                            var tenantId = _currentTenantService?.TenantId;
                            if (tenantId is not null && tenantId != Guid.Empty)
                            {
                                tenantProp.CurrentValue = tenantId;
                            }
                        }
                        catch
                        {
                            // ignore if tenant service not available at design-time or other contexts
                        }
                    }
                }

                // If the CLR properties exist, set them directly
                var createdOnProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedOn");
                if (createdOnProp != null)
                {
                    createdOnProp.CurrentValue = now;
                }
                var createdByProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedBy");
                if (createdByProp != null)
                {
                    createdByProp.CurrentValue = userId;
                }
            }

            // Consider entities modified either by state or by any modified property
            if (entry.State == EntityState.Modified || entry.Properties.Any(p => p.IsModified))
            {
                var modifiedOnProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "ModifiedOn");
                if (modifiedOnProp != null)
                {
                    modifiedOnProp.CurrentValue = now;
                }
                var modifiedByProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "ModifiedBy");
                if (modifiedByProp != null)
                {
                    modifiedByProp.CurrentValue = userId;
                }
            }
        }
    }
}
