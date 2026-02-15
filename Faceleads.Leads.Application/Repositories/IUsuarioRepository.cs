using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.Repositories;

public interface IUsuarioRepository
{
    Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default);

    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Usuario?> GetByNormalizedEmailAsync(Guid tenantId, string normalizedEmail, CancellationToken cancellationToken = default);

    Task<Usuario?> GetByNormalizedUsernameAsync(Guid tenantId, string normalizedUsername, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the usuario aggregate including roles (and related role entities).
    /// Use a tracking entity when you intend to perform modifications on the aggregate.
    /// </summary>
    Task<Usuario?> GetWithRolesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a usuario aggregate by normalized username including roles and related role entities.
    /// Typically used for authentication so role claims can be emitted.
    /// </summary>
    Task<Usuario?> GetWithRolesByNormalizedUsernameAsync(Guid tenantId, string normalizedUsername, CancellationToken cancellationToken = default);

    Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
