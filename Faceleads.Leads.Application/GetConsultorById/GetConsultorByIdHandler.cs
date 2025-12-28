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
            return Result<Consultor>.Fail(
                "CONSULTOR_ID_INVALIDO",
                "O identificador do consultor é inválido.");
        }

        var consultor = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (consultor is null)
        {
            return Result<Consultor>.Fail(
                "CONSULTOR_NAO_ENCONTRADO",
                "Consultor não encontrado.");
        }

        return Result<Consultor>.Ok(consultor);
    }
}
