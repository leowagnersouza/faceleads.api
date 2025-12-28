using Faceleads.Leads.Application.Common;
using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.CreateConsultor;

public sealed class CreateConsultorHandler
{
    private readonly IConsultorRepository _repository;

    public CreateConsultorHandler(IConsultorRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Consultor>> HandleAsync(CreateConsultorCommand command, CancellationToken cancellationToken = default)
    {
        // Validações básicas. Em um cenário real, poderíamos ter um validador dedicado.
        if (string.IsNullOrWhiteSpace(command.NomeCompleto))
        {
            return Result<Consultor>.Fail(
                "CONSULTOR_NOME_OBRIGATORIO",
                "Nome completo do consultor é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return Result<Consultor>.Fail(
                "CONSULTOR_EMAIL_OBRIGATORIO",
                "Email do consultor é obrigatório.");
        }

        var consultor = new Consultor(
            command.NomeCompleto,
            command.Email,
            command.Telefone);

        await _repository.AddAsync(consultor, cancellationToken).ConfigureAwait(false);

        return Result<Consultor>.Ok(consultor);
    }
}
