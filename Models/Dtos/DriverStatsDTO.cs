namespace IRacingLeague.Models.Dtos;

public sealed record DriverStatsDTO(
    int Races,
    int Wins,
    int Podiums,
    int Dnfs,
    decimal? AverageFinish,
    decimal? AverageIncidents,
    int FastestLaps,
    IReadOnlyList<RatingHistoryPoint> IRatingHistory,
    IReadOnlyList<RatingHistoryPoint> SafetyRatingHistory);

public sealed record RatingHistoryPoint(int RaceId, int LeagueId, string LeagueName, string Track, DateTime Date, decimal Value, decimal Change);
