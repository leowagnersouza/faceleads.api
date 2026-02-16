using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.UpdateConsultor;

public sealed class UpdateConsultorHandler
{
    private readonly IConsultorRepository _repository;

    public UpdateConsultorHandler(IConsultorRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> HandleAsync(UpdateConsultorCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty)
        {
            return Result.Fail(Errors.ConsultorIdInvalido);
        }

        if (string.IsNullOrWhiteSpace(command.NomeCompleto))
        {
            return Result.Fail(Errors.ConsultorNomeObrigatorio);
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return Result.Fail(Errors.ConsultorEmailObrigatorio);
        }

        var updated = await _repository.UpdateAsync(command.Id, command.NomeCompleto, command.Email, command.Telefone, cancellationToken).ConfigureAwait(false);

        if (!updated)
        {
            return Result.Fail(Errors.ConsultorNaoEncontrado);
        }

        return Result.Ok();
    }
}
