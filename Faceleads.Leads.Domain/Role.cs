using System.Collections.Generic;

namespace Faceleads.Leads.Domain;

public sealed class Role
{
    public Guid Id { get; private set; }

    // TenantId nulo indica role global; se não nulo, role é scoped ao tenant
    public Guid? TenantId { get; private set; }

    // Nome da role
    public string Nome { get; private set; } = string.Empty;
    public string NormalizedNome { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    // Indica se a role é de sistema e não deve ser removida
    public bool Estatico { get; private set; }

    // Auditoria
    public DateTime? CreatedOn { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedOn { get; private set; }
    public string? ModifiedBy { get; private set; }

    // Navegações
    public IReadOnlyCollection<UsuarioRole> Usuarios { get; private set; } = new List<UsuarioRole>();
    public IReadOnlyCollection<RolePermissao> Permissoes { get; private set; } = new List<RolePermissao>();

    private Role()
    {
        // Requerido pelo EF
    }

    public Role(string nome, Guid? tenantId = null, string? descricao = null, bool ehEstatico = false)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Nome = nome;
        NormalizedNome = nome.ToUpperInvariant();
        Descricao = descricao;
        Estatico = ehEstatico;
    }

    public void AtualizarNome(string nome)
    {
        Nome = nome;
        NormalizedNome = nome.ToUpperInvariant();
    }

    public void AtualizarDescricao(string? descricao)
    {
        Descricao = descricao;
    }

    public void MarcarEstatico(bool ehEstatico)
    {
        Estatico = ehEstatico;
    }
}
