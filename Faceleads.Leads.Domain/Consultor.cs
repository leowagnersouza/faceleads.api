namespace Faceleads.Leads.Domain;

public sealed class Consultor
{
    public Guid Id { get; private set; }

    public string NomeCompleto { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? Telefone { get; private set; }

    public bool Ativo { get; private set; }

    public DateTime CriadoEmUtc { get; private set; }

    // Soft delete flag
    public bool IsDeleted { get; private set; }

    // Navegação somente leitura para manter o histórico de leads
    public IReadOnlyCollection<LeadConsultor> Leads { get; private set; } = new List<LeadConsultor>();

    private Consultor()
    {
        // Requerido pelo EF Core
    }

    public Consultor(string nomeCompleto, string email, string? telefone)
    {
        Id = Guid.NewGuid();
        NomeCompleto = nomeCompleto;
        Email = email;
        Telefone = telefone;
        Ativo = true;
        CriadoEmUtc = DateTime.UtcNow;
        IsDeleted = false;
    }

    public void Desativar()
    {
        Ativo = false;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void AtualizarContato(string nomeCompleto, string email, string? telefone)
    {
        NomeCompleto = nomeCompleto;
        Email = email;
        Telefone = telefone;
    }

    public void SoftDelete()
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
    }
}

public interface IConsultorRepository
{
    Task AddAsync(Consultor consultor, CancellationToken cancellationToken = default);

    Task<Consultor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Consultor>> ListAsync(CancellationToken cancellationToken = default);

    Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
