namespace IRacingLeague.Models.Dtos;

public sealed class MeProfileDTO
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Tag { get; init; } = string.Empty;
    public int IRating { get; init; }
    public decimal SR { get; init; }
    public string License { get; init; } = string.Empty;
    public int Wins { get; init; }
    public int RacesCompleted { get; init; }
    public static MeProfileDTO FromEntity(User u) => new() { Id = u.UserId, Username = u.UserName, Email = u.Email, Tag = u.Tag, IRating = u.IRating, SR = u.SafetyRating, License = u.LicenseClass, Wins = u.TotalWins, RacesCompleted = u.RacesCompleted };
}
