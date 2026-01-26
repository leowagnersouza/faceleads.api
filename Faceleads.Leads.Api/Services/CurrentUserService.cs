using Faceleads.Leads.Application.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Faceleads.Leads.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId
    {
        get
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null) return null;

            // Try to read from NameIdentifier claim first
            var id = ctx.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(id)) return id;

            // Fallback to Name
            id = ctx.User?.Identity?.Name;
            return id;
        }
    }
}
