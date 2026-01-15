using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.ListConsultores;

public sealed class ListConsultoresHandler
{
    private readonly IConsultorRepository _repository;

    public ListConsultoresHandler(IConsultorRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<Consultor>>> HandleAsync(ListConsultoresQuery query, CancellationToken cancellationToken = default)
    {
        var consultores = await _repository.ListAsync(cancellationToken).ConfigureAwait(false);

        return Result<IEnumerable<Consultor>>.Ok(consultores);
    }
}
