using System.Security.Cryptography;
using System.Text;

namespace IRacingLeague.Models;

// Single source of truth for password hashing. SHA256, hex-encoded, lowercase.
// Used by registration, login verification, the seeder and UserService.Create so
// every persisted password hash is produced identically.
public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
