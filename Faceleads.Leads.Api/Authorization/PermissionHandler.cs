using Microsoft.AspNetCore.Authorization;
using Faceleads.Leads.Application.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Faceleads.Leads.Api.Authorization;

public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IRolePermissaoRepository _rolePermRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IMemoryCache _cache;

    public PermissionHandler(IUsuarioRepository usuarioRepo, IRolePermissaoRepository rolePermRepo, IRoleRepository roleRepo, IMemoryCache cache)
    {
        _usuarioRepo = usuarioRepo;
        _rolePermRepo = rolePermRepo;
        _roleRepo = roleRepo;
        _cache = cache;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // Try to read roles from claims
        var roles = context.User?.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray() ?? Array.Empty<string>();

        if (!roles.Any())
        {
            // No roles in token; try to load user and roles from DB using NameIdentifier or Name claim
            var idClaim = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(idClaim))
            {
                context.Fail();
                return;
            }

            if (!Guid.TryParse(idClaim, out var userId))
            {
                context.Fail();
                return;
            }

            var usuario = await _usuarioRepo.GetWithRolesAsync(userId);
            if (usuario is null)
            {
                context.Fail();
                return;
            }

            roles = usuario.Roles?.Select(r => r.Role!.Nome).ToArray() ?? Array.Empty<string>();
        }

        // For each role, check cached permissions (cache key includes tenant to avoid cross-tenant collisions)
        foreach (var role in roles)
        {
            // Normalize role and permission names for consistent comparisons
            var normalizedRole = role.ToUpperInvariant();
            var normalizedRequirement = requirement.Permission.ToUpperInvariant();

            // Try to read tenant id from claims so we resolve tenant-scoped roles correctly
            Guid? tenantId = null;
            var tenantClaim = context.User?.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(tenantClaim) && Guid.TryParse(tenantClaim, out var parsedTenant))
            {
                tenantId = parsedTenant;
            }

            var tenantKey = tenantId?.ToString() ?? "global";
            var cacheKey = $"role_perms:{tenantKey}:{normalizedRole}";

            if (!_cache.TryGetValue<HashSet<string>>(cacheKey, out var perms))
            {
                // Resolve role id by normalized name then load permissions for that role
                var roleEntity = await _roleRepo.GetByNormalizedNameAsync(tenantId, normalizedRole);
                if (roleEntity is null)
                {
                    perms = new HashSet<string>();
                }
                else
                {
                    var permissions = await _rolePermRepo.GetPermissoesForRoleAsync(roleEntity.Id);
                    // Ensure permission names are normalized the same way
                    perms = permissions?.Select(p => p.NormalizedNome.ToUpperInvariant()).ToHashSet() ?? new HashSet<string>();
                }

                _cache.Set(cacheKey, perms, TimeSpan.FromMinutes(5));
            }

            if (perms.Contains(normalizedRequirement))
            {
                context.Succeed(requirement);
                return;
            }
        }

        context.Fail();
    }
}
