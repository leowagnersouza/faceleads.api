namespace Faceleads.Leads.Application.UpdateUsuario;

public sealed class UpdateUsuarioCommand
{
    public Guid Id { get; init; }

    // Optional fields for PATCH semantics — null means "do not modify"
    public string? NomeUsuario { get; init; }
    public string? Email { get; init; }
    public string? Password { get; init; }
    public Guid? ConsultorId { get; init; }
    public bool? Ativo { get; init; }
}
