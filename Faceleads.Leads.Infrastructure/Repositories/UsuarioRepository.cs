using Faceleads.Leads.Application.Repositories;
using Faceleads.Leads.Domain;
using Microsoft.EntityFrameworkCore;

namespace Faceleads.Leads.Infrastructure.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly LeadsDbContext _db;

    public UsuarioRepository(LeadsDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _db.Set<Usuario>().AddAsync(usuario, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Usuario?> GetByNormalizedEmailAsync(Guid tenantId, string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return await _db.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.TenantId == tenantId && u.NormalizedEmail == normalizedEmail, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Usuario?> GetByNormalizedUsernameAsync(Guid tenantId, string normalizedUsername, CancellationToken cancellationToken = default)
    {
        return await _db.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.TenantId == tenantId && u.NormalizedNomeUsuario == normalizedUsername, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Usuario?> GetWithRolesByNormalizedUsernameAsync(Guid tenantId, string normalizedUsername, CancellationToken cancellationToken = default)
    {
        return await _db.Usuarios
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.TenantId == tenantId && u.NormalizedNomeUsuario == normalizedUsername, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Usuario?> GetWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Usuarios
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Usuario>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Usuarios.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
