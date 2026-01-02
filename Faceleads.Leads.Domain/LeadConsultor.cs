namespace Faceleads.Leads.Domain;

public sealed class LeadConsultor
{
    public Guid Id { get; private set; }

    public Guid LeadId { get; private set; }

    public Guid? ConsultorId { get; private set; }

    public DateTime AtribuidoEmUtc { get; private set; }

    public DateTime? EncerradoEmUtc { get; private set; }

    public Lead Lead { get; private set; } = null!;

    public Consultor? Consultor { get; private set; }

    private LeadConsultor()
    {
        // Requerido pelo EF Core
    }

    public LeadConsultor(Guid leadId, Guid consultorId)
    {
        Id = Guid.NewGuid();
        LeadId = leadId;
        ConsultorId = consultorId;
        AtribuidoEmUtc = DateTime.UtcNow;
    }

    public void Encerrar()
    {
        if (EncerradoEmUtc is not null)
        {
            return;
        }

        EncerradoEmUtc = DateTime.UtcNow;
    }
}
