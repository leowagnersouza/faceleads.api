using Faceleads.Leads.Domain;
using Microsoft.EntityFrameworkCore;

namespace Faceleads.Leads.Infrastructure;

public sealed class LeadRepository : ILeadRepository
{
    private readonly LeadsDbContext _dbContext;

    public LeadRepository(LeadsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        await _dbContext.Leads.AddAsync(lead, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }
}
