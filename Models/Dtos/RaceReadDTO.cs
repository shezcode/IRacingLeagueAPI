namespace IRacingLeague.Models.Dtos;

public class RaceReadDTO
{
    public int RaceId { get; set; }
    public int LeagueId { get; set; }
    public string Track { get; set; } = string.Empty;
    public string Car { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public int LapCount { get; set; }
    public bool IsCompleted { get; set; }
    public decimal AmbientTempC { get; set; }
    public int Round { get; set; }

    public static RaceReadDTO FromEntity(Race race) => new()
    {
        RaceId = race.RaceId,
        LeagueId = race.LeagueId,
        Track = race.Track,
        Car = race.Car,
        ScheduledAt = race.ScheduledAt,
        LapCount = race.LapCount,
        IsCompleted = race.IsCompleted,
        AmbientTempC = race.AmbientTempC,
        Round = race.Round
    };
}
