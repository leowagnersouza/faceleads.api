namespace Faceleads.Leads.Domain;

public sealed class RolePermissao
{
    public Guid RoleId { get; private set; }

    public Guid PermissaoId { get; private set; }

    // Auditoria
    public DateTime? CreatedOn { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedOn { get; private set; }
    public string? ModifiedBy { get; private set; }

    // Navegações
    public Role? Role { get; private set; }
    public Permissao? Permissao { get; private set; }

    private RolePermissao()
    {
        // EF
    }

    public RolePermissao(Guid roleId, Guid permissaoId, string? createdBy = null)
    {
        RoleId = roleId;
        PermissaoId = permissaoId;
    }
}
