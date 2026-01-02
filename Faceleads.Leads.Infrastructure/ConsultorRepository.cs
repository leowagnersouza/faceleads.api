using Faceleads.Leads.Domain;
using Microsoft.EntityFrameworkCore;

namespace Faceleads.Leads.Infrastructure;

public sealed class ConsultorRepository : IConsultorRepository
{
    private readonly LeadsDbContext _dbContext;

    public ConsultorRepository(LeadsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Consultor consultor, CancellationToken cancellationToken = default)
    {
        await _dbContext.Consultores.AddAsync(consultor, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Consultor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Consultores
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Consultor>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Consultores
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var consultor = await _dbContext.Consultores
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (consultor is null)
        {
            return false;
        }

        consultor.Ativar();
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var consultor = await _dbContext.Consultores
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (consultor is null)
        {
            return false;
        }

        consultor.Desativar();
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var consultor = await _dbContext.Consultores
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (consultor is null)
        {
            return false;
        }

        consultor.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
