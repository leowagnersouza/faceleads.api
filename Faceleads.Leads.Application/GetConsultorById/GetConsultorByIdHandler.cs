using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.GetConsultorById;

public sealed class GetConsultorByIdHandler
{
    private readonly IConsultorRepository _repository;

    public GetConsultorByIdHandler(IConsultorRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Consultor>> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Result<Consultor>.Fail(Errors.ConsultorIdInvalido);
        }

        var consultor = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (consultor is null)
        {
            return Result<Consultor>.Fail(Errors.ConsultorNaoEncontrado);
        }

        return Result<Consultor>.Ok(consultor);
    }
}
