using System.Collections.Generic;

namespace Faceleads.Leads.Domain;

public sealed class Permissao
{
    public Guid Id { get; private set; }

    // Nome canônico da permissão, ex.: "consultor.delete"
    public string Nome { get; private set; } = string.Empty;
    public string NormalizedNome { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    // Auditoria
    public DateTime? CreatedOn { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedOn { get; private set; }
    public string? ModifiedBy { get; private set; }

    // Navegações
    public IReadOnlyCollection<RolePermissao> Roles { get; private set; } = new List<RolePermissao>();

    private Permissao()
    {
        // Requerido pelo EF Core
    }

    public Permissao(string nome, string? descricao = null)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        NormalizedNome = nome.ToUpperInvariant();
        Descricao = descricao;
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
}
