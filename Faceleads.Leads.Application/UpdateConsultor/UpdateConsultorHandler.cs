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
            return Result.Fail("CONSULTOR_ID_INVALIDO", "O identificador do consultor é inválido.");
        }

        if (string.IsNullOrWhiteSpace(command.NomeCompleto))
        {
            return Result.Fail("CONSULTOR_NOME_OBRIGATORIO", "Nome completo do consultor é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return Result.Fail("CONSULTOR_EMAIL_OBRIGATORIO", "Email do consultor é obrigatório.");
        }

        var updated = await _repository.UpdateAsync(command.Id, command.NomeCompleto, command.Email, command.Telefone, cancellationToken).ConfigureAwait(false);

        if (!updated)
        {
            return Result.Fail("CONSULTOR_NAO_ENCONTRADO", "Consultor não encontrado.");
        }

        return Result.Ok();
    }
}
