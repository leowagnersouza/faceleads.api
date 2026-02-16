using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.ActivateConsultor;

public sealed class ActivateConsultorHandler
{
    private readonly IConsultorRepository _repo;

    public ActivateConsultorHandler(IConsultorRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> HandleAsync(ActivateConsultorCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty)
        {
            return Result.Fail(Errors.ConsultorIdInvalido);
        }

        var ok = await _repo.ActivateAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (!ok)
        {
            return Result.Fail(Errors.ConsultorNaoEncontrado);
        }

        return Result.Ok();
    }
}
