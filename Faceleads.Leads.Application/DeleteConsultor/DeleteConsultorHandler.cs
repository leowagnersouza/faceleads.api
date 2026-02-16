using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.DeleteConsultor;

public sealed class DeleteConsultorHandler
{
    private readonly IConsultorRepository _repository;

    public DeleteConsultorHandler(IConsultorRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> HandleAsync(DeleteConsultorCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty)
        {
            return Result.Fail(Errors.ConsultorIdInvalido);
        }

        var success = await _repository.SoftDeleteAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (!success)
        {
            return Result.Fail(Errors.ConsultorNaoEncontrado);
        }

        return Result.Ok();
    }
}
