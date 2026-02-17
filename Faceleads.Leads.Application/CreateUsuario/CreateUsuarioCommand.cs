namespace Faceleads.Leads.Application.CreateUsuario;

public sealed class CreateUsuarioCommand
{
    public string NomeUsuario { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public Guid? ConsultorId { get; init; }
}
