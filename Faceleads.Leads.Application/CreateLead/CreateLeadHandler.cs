using Faceleads.Leads.Domain;

namespace Faceleads.Leads.Application.CreateLead;

public sealed class CreateLeadHandler
{
    private readonly ILeadRepository _repository;

    public CreateLeadHandler(ILeadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> HandleAsync(CreateLeadCommand command, CancellationToken cancellationToken = default)
    {
        var sourceParsed = Enum.TryParse<LeadSource>(command.Source, ignoreCase: true, out var source)
            ? source
            : LeadSource.Other;

        var lead = new Lead(
            command.FullName,
            command.Email,
            command.Phone,
            sourceParsed);

        await _repository.AddAsync(lead, cancellationToken).ConfigureAwait(false);

        return lead.Id;
    }
}
