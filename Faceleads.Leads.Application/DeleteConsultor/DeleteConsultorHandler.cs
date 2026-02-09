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
            return Result.Fail("CONSULTOR_ID_INVALIDO", "O identificador do consultor é inválido.");
        }

        var success = await _repository.SoftDeleteAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (!success)
        {
            return Result.Fail("CONSULTOR_NAO_ENCONTRADO", "Consultor não encontrado.");
        }

        return Result.Ok();
    }
}
