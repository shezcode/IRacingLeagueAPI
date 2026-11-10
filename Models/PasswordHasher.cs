using Microsoft.AspNetCore.Identity;

namespace IRacingLeague.Models;

// ASP.NET Identity's format includes a version, per-password salt, and work factor.
public static class PasswordHasher
{
    private static readonly PasswordHasher<object> Hasher = new();
    public static string Hash(string password) => Hasher.HashPassword(new object(), password);
    public static bool Verify(string hashedPassword, string password) =>
        Hasher.VerifyHashedPassword(new object(), hashedPassword, password) != PasswordVerificationResult.Failed;
}
