using IRacingLeague.Models;

namespace IRacingLeague.Business;

public sealed class RatingCalculator
{
    public const int EloK = 200;
    public const int BaseIRating = 1350;
    public const decimal BaseSafetyRating = 2.50m;

    public IReadOnlyList<(Result Result, int IRatingChange, decimal SafetyRatingChange)> CalculateRace(
        IReadOnlyList<(Result Result, int UserId, int IRating, decimal SafetyRating)> field)
    {
        if (field.Count <= 1)
            return field.Select(x => (x.Result, 0, AppliedSafetyChange(x.SafetyRating, x.Result.IncidentPoints))).ToArray();

        return field.Select(driver =>
        {
            var expected = field.Where(other => other.Result.ResultId != driver.Result.ResultId)
                .Sum(other => 1d / (1d + Math.Pow(10d, (other.IRating - driver.IRating) / 400d)));
            var actual = field.Count(other => other.Result.Position > driver.Result.Position);
            var rawChange = (int)Math.Round(EloK * (actual - expected) / (field.Count - 1), MidpointRounding.AwayFromZero);
            var appliedIRatingChange = Math.Max(0, driver.IRating + rawChange) - driver.IRating;
            return (driver.Result, appliedIRatingChange, AppliedSafetyChange(driver.SafetyRating, driver.Result.IncidentPoints));
        }).ToArray();
    }

    private static decimal AppliedSafetyChange(decimal before, int incidents)
    {
        var after = Math.Clamp(before + 0.10m - 0.05m * incidents, 0m, 4.99m);
        return after - before;
    }
}
