namespace IRacingLeague.Models;

public class User
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;   // SHA256 hash in the API; plain placeholder in the CLI
    public string Role { get; set; } = "Driver";            // Admin / Driver
    public string Tag { get; set; } = string.Empty;
    public int IRating { get; set; }                        // global, recomputed from results
    public decimal SafetyRating { get; set; }
    public string LicenseClass { get; set; } = "Rookie";    // Rookie / D / C / B / A
    public bool IsActive { get; set; }
    public int TotalWins { get; set; }                      // global, recomputed
    public DateTime JoinedAt { get; set; }
    public int RacesCompleted { get; set; }

    // Parameterless ctor kept for JSON (de)serialization in a later step.
    public User() { }

    public User(string userName, string email, string password, string role, string tag, string licenseClass)
    {
        UserName = userName;
        Email = email;
        Password = password;
        Role = role;
        Tag = tag;
        LicenseClass = licenseClass;
        IRating = 1350;          // iRacing's default starting iRating
        SafetyRating = 2.50m;
        TotalWins = 0;
        IsActive = true;
        JoinedAt = DateTime.UtcNow;
    }

}
