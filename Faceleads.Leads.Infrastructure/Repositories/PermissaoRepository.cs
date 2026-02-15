using Faceleads.Leads.Application.Repositories;
using Faceleads.Leads.Domain;
using Microsoft.EntityFrameworkCore;

namespace Faceleads.Leads.Infrastructure.Repositories;

public sealed class PermissaoRepository : IPermissaoRepository
{
    private readonly LeadsDbContext _db;

    public PermissaoRepository(LeadsDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Permissao permissao, CancellationToken cancellationToken = default)
    {
        await _db.Set<Permissao>().AddAsync(permissao, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Permissao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Permissoes.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Permissao?> GetByNameAsync(string nome, CancellationToken cancellationToken = default)
    {
        var normalized = nome.ToUpperInvariant();
        return await _db.Permissoes.AsNoTracking().SingleOrDefaultAsync(p => p.NormalizedNome == normalized, cancellationToken).ConfigureAwait(false);
    }
}
