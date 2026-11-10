namespace IRacingLeague.Models;

public class Registration
{
    public int RegistrationId { get; set; }
    public int UserId { get; set; }      // FK -> User
    public int LeagueId { get; set; }    // FK -> League
    public int CarNumber { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public decimal BallastKg { get; set; }
    public bool IsActive { get; set; }
    public DateTime JoinedLeagueAt { get; set; }

    // Parameterless ctor kept for JSON (de)serialization in a later step.
    public Registration() { }

    public Registration(int userId, int leagueId, int carNumber, string teamName, decimal ballastKg)
    {
        UserId = userId;
        LeagueId = leagueId;
        CarNumber = carNumber;
        TeamName = teamName;
        BallastKg = ballastKg;
        IsActive = true;
        JoinedLeagueAt = DateTime.UtcNow;
    }

}
