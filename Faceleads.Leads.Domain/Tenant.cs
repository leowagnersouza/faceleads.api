namespace Faceleads.Leads.Domain;

public sealed class Tenant
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    public bool Ativo { get; private set; }


    private Tenant()
    {
        // requerido pelo EF Core
    }

    public Tenant(string nome, string? descricao = null)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Descricao = descricao;
        Ativo = true;
    }

    public void Desativar()
    {
        Ativo = false;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void AtualizarDescricao(string? descricao)
    {
        Descricao = descricao;
    }

    // Auditing properties
    public DateTime? CreatedOn { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedOn { get; private set; }
    public string? ModifiedBy { get; private set; }
}

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
