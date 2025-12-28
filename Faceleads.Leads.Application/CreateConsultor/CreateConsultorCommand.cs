namespace Faceleads.Leads.Application.CreateConsultor;

public sealed class CreateConsultorCommand
{
    public string NomeCompleto { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Telefone { get; init; }
}
