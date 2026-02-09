namespace Faceleads.Leads.Application.UpdateConsultor;

public sealed class UpdateConsultorCommand
{
    public Guid Id { get; init; }

    public string NomeCompleto { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Telefone { get; init; }
}
