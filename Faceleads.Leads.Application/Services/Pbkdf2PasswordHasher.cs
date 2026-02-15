using System;
using System.Security.Cryptography;
using System.Text;

namespace Faceleads.Leads.Application.Services;

public sealed class Pbkdf2PasswordHasher<T> : IPasswordHasher<T>
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string HashPassword(T user, string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var derive = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var key = derive.GetBytes(KeySize);

        var parts = new[] { Iterations.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(key) };
        return string.Join(':', parts);
    }

    public bool VerifyHashedPassword(T user, string hashedPassword, string providedPassword)
    {
        try
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 3) return false;
            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            using var derive = new Rfc2898DeriveBytes(providedPassword, salt, iterations, HashAlgorithmName.SHA256);
            var keyToCheck = derive.GetBytes(key.Length);
            return CryptographicOperations.FixedTimeEquals(keyToCheck, key);
        }
        catch
        {
            return false;
        }
    }
}
