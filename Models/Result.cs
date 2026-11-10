namespace IRacingLeague.Models;

public class Result
{
    public int ResultId { get; set; }
    public int RegistrationId { get; set; }   // FK -> Registration
    public int RaceId { get; set; }           // FK -> Race
    public int Position { get; set; }
    public decimal FastestLapSeconds { get; set; }
    public bool FastestLap { get; set; }
    public int Points { get; set; }           // points awarded for this race
    public int IncidentPoints { get; set; }   // iRacing "x" incidents
    public int IRatingBefore { get; set; }
    public int IRatingChange { get; set; }
    public decimal SafetyRatingBefore { get; set; }
    public decimal SafetyRatingChange { get; set; }
    public bool Dnf { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime FinishedAt { get; set; }

    // Parameterless ctor kept for JSON (de)serialization in a later step.
    public Result() { }

    public Result(int registrationId, int raceId, int position, decimal fastestLapSeconds,
                  int points, int incidentPoints, bool dnf, string notes)
    {
        RegistrationId = registrationId;
        RaceId = raceId;
        Position = position;
        FastestLapSeconds = fastestLapSeconds;
        Points = points;
        IncidentPoints = incidentPoints;
        Dnf = dnf;
        Notes = notes;
        FinishedAt = DateTime.UtcNow;
    }

}
