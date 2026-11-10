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
    public int Wins { get; set; }
    public int Podiums { get; set; }
    public int IncidentPoints { get; set; }
    public int BestFinish { get; set; }

    public static RegistrationReadDTO FromEntity(Registration r, int racesCompleted = 0) => new()
    {
        RegistrationId = r.RegistrationId,
        UserId = r.UserId,
        LeagueId = r.LeagueId,
        CarNumber = r.CarNumber,
        TeamName = r.TeamName,
        Points = 0,
        BallastKg = r.BallastKg,
        IsActive = r.IsActive,
        JoinedLeagueAt = r.JoinedLeagueAt,
        RacesCompleted = racesCompleted
    };

    public static RegistrationReadDTO FromStanding(StandingSummary s, int position) => new()
    {
        RegistrationId = s.Registration.RegistrationId, UserId = s.Registration.UserId, LeagueId = s.Registration.LeagueId,
        CarNumber = s.Registration.CarNumber, TeamName = s.Registration.TeamName, Points = s.Points,
        BallastKg = s.Registration.BallastKg, IsActive = s.Registration.IsActive, JoinedLeagueAt = s.Registration.JoinedLeagueAt,
        RacesCompleted = s.RacesCompleted, Wins = s.Wins, Podiums = s.Podiums, IncidentPoints = s.IncidentPoints,
        BestFinish = s.BestFinish, Position = position
    };
    public int Position { get; set; }
}
