namespace IRacingLeague.Models.Dtos;

public class ResultReadDTO
{
    public int ResultId { get; set; }
    public int RegistrationId { get; set; }
    public int RaceId { get; set; }
    public int Position { get; set; }
    public decimal FastestLapSeconds { get; set; }
    public int Points { get; set; }
    public int IncidentPoints { get; set; }
    public bool Dnf { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime FinishedAt { get; set; }

    public static ResultReadDTO FromEntity(Result r) => new()
    {
        ResultId = r.ResultId,
        RegistrationId = r.RegistrationId,
        RaceId = r.RaceId,
        Position = r.Position,
        FastestLapSeconds = r.FastestLapSeconds,
        Points = r.Points,
        IncidentPoints = r.IncidentPoints,
        Dnf = r.Dnf,
        Notes = r.Notes,
        FinishedAt = r.FinishedAt
    };
}
