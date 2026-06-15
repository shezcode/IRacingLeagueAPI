namespace IRacingLeague.Models.Dtos;

public class RegistrationReadDTO
{
    public int RegistrationId { get; set; }
    public int UserId { get; set; }
    public int LeagueId { get; set; }
    public int CarNumber { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int Points { get; set; }
    public decimal BallastKg { get; set; }
    public bool IsActive { get; set; }
    public DateTime JoinedLeagueAt { get; set; }
    public int RacesCompleted { get; set; }

    public static RegistrationReadDTO FromEntity(Registration r, int racesCompleted = 0) => new()
    {
        RegistrationId = r.RegistrationId,
        UserId = r.UserId,
        LeagueId = r.LeagueId,
        CarNumber = r.CarNumber,
        TeamName = r.TeamName,
        Points = r.Points,
        BallastKg = r.BallastKg,
        IsActive = r.IsActive,
        JoinedLeagueAt = r.JoinedLeagueAt,
        RacesCompleted = racesCompleted
    };
}
