namespace Faceleads.Leads.Api.Requests;

public sealed class UpdateConsultorRequest
{
    public string NomeCompleto { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Telefone { get; init; }
}
