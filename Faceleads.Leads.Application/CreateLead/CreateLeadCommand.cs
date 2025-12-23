namespace Faceleads.Leads.Application.CreateLead;

public sealed class CreateLeadCommand
{
    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Phone { get; init; }

    public string Source { get; init; } = string.Empty;
}
