namespace Faceleads.Leads.Domain;

public sealed class Tenant
{
    public Guid Id { get; private set; }
    // Nome amigável para uso interno
    public string Nome { get; private set; } = string.Empty;

    // Descrição opcional
    public string? Descricao { get; private set; }

    public bool Ativo { get; private set; }

    public DateTime CriadoEmUtc { get; private set; }

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
        CriadoEmUtc = DateTime.UtcNow;
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
}
