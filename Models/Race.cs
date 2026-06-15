namespace IRacingLeague.Models;

public class Race
{
    public int RaceId { get; set; }
    public int LeagueId { get; set; }   // FK -> League
    public string Track { get; set; } = string.Empty;
    public string Car { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public int LapCount { get; set; }
    public bool IsCompleted { get; set; }
    public decimal AmbientTempC { get; set; }
    public int Round { get; set; }

    // Parameterless ctor kept for JSON (de)serialization in a later step.
    public Race() { }

    // Round is intentionally not a constructor parameter: it is derived from schedule
    // order and assigned by the race service after every create/delete, never set by callers.
    public Race(int leagueId, string track, string car, DateTime scheduledAt, int lapCount, decimal ambientTempC)
    {
        LeagueId = leagueId;
        Track = track;
        Car = car;
        ScheduledAt = scheduledAt;
        LapCount = lapCount;
        AmbientTempC = ambientTempC;
        IsCompleted = false;
    }

    public override string ToString() =>
        $"[{RaceId}] R{Round} {Track} ({Car}) — {ScheduledAt:yyyy-MM-dd HH:mm}, " +
        $"{LapCount} laps, {AmbientTempC:0.#}C{(IsCompleted ? " (completed)" : "")}";
}
