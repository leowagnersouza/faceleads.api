using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.DeactivateConsultor;

public sealed class DeactivateConsultorHandler
{
    private readonly IConsultorRepository _repo;

    public DeactivateConsultorHandler(IConsultorRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> HandleAsync(DeactivateConsultorCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty)
        {
            return Result.Fail("CONSULTOR_ID_INVALIDO", "O identificador do consultor é inválido.");
        }

        var ok = await _repo.DeactivateAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (!ok)
        {
            return Result.Fail("CONSULTOR_NAO_ENCONTRADO", "Consultor não encontrado.");
        }

        return Result.Ok();
    }
}
