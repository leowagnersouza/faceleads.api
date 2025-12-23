namespace Faceleads.Leads.Domain;

public enum LeadSource
{
    Facebook = 1,
    Instagram = 2,
    GoogleAds = 3,
    Other = 99
}

public enum LeadStatus
{
    New = 1,
    InProgress = 2,
    Converted = 3,
    Lost = 4
}

public sealed class Lead
{
    public Guid Id { get; private set; }

    public string NomeCompleto { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? Telefone { get; private set; }

    public LeadSource Origem { get; private set; }

    public LeadStatus Status { get; private set; }

    public DateTime CriadoEmUtc { get; private set; }

    public DateTime? AtribuidoEmUtc { get; private set; }

    private Lead()
    {
        // Required by EF Core
    }

    public Lead(
        string nomeCompleto,
        string email,
        string? telefone,
        LeadSource origem)
    {
        Id = Guid.NewGuid();
        NomeCompleto = nomeCompleto;
        Email = email;
        Telefone = telefone;
        Origem = origem;
        Status = LeadStatus.New;
        CriadoEmUtc = DateTime.UtcNow;
    }

    public void Assign()
    {
        if (Status != LeadStatus.New)
        {
            return;
        }

        Status = LeadStatus.InProgress;
        AtribuidoEmUtc = DateTime.UtcNow;
    }

    public void MarkAsConverted()
    {
        if (Status == LeadStatus.Converted)
        {
            return;
        }

        Status = LeadStatus.Converted;
    }

    public void MarkAsLost()
    {
        if (Status == LeadStatus.Lost)
        {
            return;
        }

        Status = LeadStatus.Lost;
    }
}

public interface ILeadRepository
{
    Task AddAsync(Lead lead, CancellationToken cancellationToken = default);

    Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
