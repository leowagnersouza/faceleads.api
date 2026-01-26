namespace Faceleads.Leads.Application.Common;

public interface ICurrentUserService
{
    // Unique identifier of current user (could be Guid string or username)
    string? UserId { get; }
}
