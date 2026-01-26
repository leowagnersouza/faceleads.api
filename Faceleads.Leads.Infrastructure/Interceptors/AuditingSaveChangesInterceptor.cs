using Faceleads.Leads.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
