using Faceleads.Leads.Application.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Faceleads.Leads.Api.Services;

public sealed class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null) throw new InvalidOperationException("No HttpContext available to resolve tenant");

            // Expect tenant id in JWT claim 'tenant_id' or in claim type NameIdentifier if used
            var tenantClaim = ctx.User?.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(tenantClaim) && Guid.TryParse(tenantClaim, out var tid)) return tid;

            var alt = ctx.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(alt) && Guid.TryParse(alt, out var altTid)) return altTid;
            // Fallback to default tenant for now (dev): e7a1f3c2-9b4d-4f6a-8c12-3b9d2f0a6e5f
            return Guid.Parse("e7a1f3c2-9b4d-4f6a-8c12-3b9d2f0a6e5f");
        }
    }
}
