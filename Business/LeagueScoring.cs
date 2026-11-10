namespace IRacingLeague.Business;

public static class LeagueScoring
{
    public static List<string> Validate(IReadOnlyList<int> pointsTable, int fastestLapBonus)
    {
        var errors = new List<string>();
        if (pointsTable.Count is < 1 or > 50) errors.Add("The points table must contain between 1 and 50 positions.");
        if (pointsTable.Any(points => points is < 0 or > 1000)) errors.Add("Each points value must be between 0 and 1000.");
        if (pointsTable.Zip(pointsTable.Skip(1)).Any(pair => pair.First < pair.Second)) errors.Add("Points cannot increase at a lower finishing position.");
        if (fastestLapBonus is < 0 or > 50) errors.Add("Fastest-lap bonus must be between 0 and 50.");
        return errors;
    }
}
