using Faceleads.Leads.Domain;
using Microsoft.EntityFrameworkCore;

namespace Faceleads.Leads.Infrastructure;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly LeadsDbContext _dbContext;

    public RefreshTokenRepository(LeadsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<RefreshToken>()
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<RefreshToken>().Update(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
