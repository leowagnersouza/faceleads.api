using Microsoft.AspNetCore.Identity;
using AppServices = Faceleads.Leads.Application.Services;

namespace Faceleads.Leads.Api.Adapters;

public sealed class PasswordHasherAdapter<T> : AppServices.IPasswordHasher<T> where T : class
{
    private readonly PasswordHasher<T> _inner = new PasswordHasher<T>();

    public string HashPassword(T user, string password) => _inner.HashPassword(user, password);

    public bool VerifyHashedPassword(T user, string hashedPassword, string providedPassword)
    {
        var result = _inner.VerifyHashedPassword(user, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
