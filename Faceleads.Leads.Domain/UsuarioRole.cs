namespace Faceleads.Leads.Domain;

public sealed class UsuarioRole
{
    public Guid UsuarioId { get; private set; }

    public Guid RoleId { get; private set; }

    // Opcional: quem atribuiu / quando
    public DateTime? AssignedAt { get; private set; }
    public string? AssignedBy { get; private set; }

    // Auditoria padrão
    public DateTime? CreatedOn { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedOn { get; private set; }
    public string? ModifiedBy { get; private set; }

    // Navegações
    public Usuario? Usuario { get; private set; }
    public Role? Role { get; private set; }

    private UsuarioRole()
    {
        // EF
    }

    public UsuarioRole(Guid usuarioId, Guid roleId, string? assignedBy = null)
    {
        UsuarioId = usuarioId;
        RoleId = roleId;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
    }
}
