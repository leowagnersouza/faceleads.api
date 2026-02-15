using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.Repositories;

public interface IPermissaoRepository
{
    Task AddAsync(Permissao permissao, CancellationToken cancellationToken = default);

    Task<Permissao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Permissao?> GetByNameAsync(string nome, CancellationToken cancellationToken = default);
}
