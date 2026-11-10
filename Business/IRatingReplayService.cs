using IRacingLeague.Data;
using IRacingLeague.Models;

namespace IRacingLeague.Business;

public interface IRatingReplayService
{
    Task ReplayFromAsync(DateTime scheduledAt, int raceId, IReadOnlyCollection<int> changedRegistrationIds, CancellationToken cancellationToken = default);
}

public sealed class RatingReplayService(IRatingReplayRepository replayRepository, IUserRepository users, RatingCalculator calculator) : IRatingReplayService
{
    public async Task ReplayFromAsync(DateTime scheduledAt, int raceId, IReadOnlyCollection<int> changedRegistrationIds, CancellationToken cancellationToken = default)
    {
        await replayRepository.AcquireGlobalLockAsync(cancellationToken);
        var affectedUserIds = (await replayRepository.GetUserIdsForRegistrationsAsync(changedRegistrationIds, cancellationToken)).ToHashSet();
        var before = await replayRepository.GetEntriesBeforeAsync(scheduledAt, raceId, cancellationToken);
        var after = await replayRepository.GetEntriesFromAsync(scheduledAt, raceId, cancellationToken);
        var states = before.GroupBy(x => x.UserId).ToDictionary(g => g.Key, g =>
        {
            var rows = g.OrderBy(x => x.ScheduledAt).ThenBy(x => x.Result.RaceId).ToList();
            var last = rows[^1].Result;
            return new DriverState(last.IRatingBefore + last.IRatingChange,
                last.SafetyRatingBefore + last.SafetyRatingChange,
                rows.Count,
                rows.Count(x => x.Result.Position == 1 && !x.Result.Dnf));
        });

        var touchedUserIds = new HashSet<int>(affectedUserIds);
        foreach (var race in after.GroupBy(x => new { x.ScheduledAt, x.Result.RaceId }))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var field = race.OrderBy(x => x.Result.Position).Select(x =>
            {
                touchedUserIds.Add(x.UserId);
                if (!states.TryGetValue(x.UserId, out var state)) state = new DriverState(RatingCalculator.BaseIRating, RatingCalculator.BaseSafetyRating, 0, 0);
                return (x, state);
            }).ToList();
            var changes = calculator.CalculateRace(field.Select(x => (x.x.Result, x.x.UserId, x.state.IRating, x.state.SafetyRating)).ToArray());
            for (var i = 0; i < field.Count; i++)
            {
                var (entry, state) = field[i];
                var change = changes[i];
                change.Result.IRatingBefore = state.IRating;
                change.Result.IRatingChange = change.IRatingChange;
                change.Result.SafetyRatingBefore = state.SafetyRating;
                change.Result.SafetyRatingChange = change.SafetyRatingChange;
                state.IRating += change.IRatingChange;
                state.SafetyRating += change.SafetyRatingChange;
                state.RacesCompleted++;
                if (entry.Result.Position == 1 && !entry.Result.Dnf) state.Wins++;
                states[entry.UserId] = state;
            }
        }

        if (touchedUserIds.Count == 0) return;
        var touchedUsers = await users.GetByIdsAsync(touchedUserIds, cancellationToken);
        foreach (var user in touchedUsers)
        {
            if (!states.TryGetValue(user.UserId, out var state)) state = new DriverState(RatingCalculator.BaseIRating, RatingCalculator.BaseSafetyRating, 0, 0);
            user.IRating = state.IRating;
            user.SafetyRating = state.SafetyRating;
            user.RacesCompleted = state.RacesCompleted;
            user.TotalWins = state.Wins;
            user.LicenseClass = LicenseFor(state.RacesCompleted, state.SafetyRating);
            await users.UpdateAsync(user, cancellationToken);
        }
    }

    private static string LicenseFor(int racesCompleted, decimal safetyRating) => racesCompleted < 5 ? "Rookie" : safetyRating < 2.0m ? "D" : safetyRating < 3.0m ? "C" : safetyRating < 4.0m ? "B" : "A";

    private sealed class DriverState(int iRating, decimal safetyRating, int racesCompleted, int wins)
    {
        public int IRating { get; set; } = iRating;
        public decimal SafetyRating { get; set; } = safetyRating;
        public int RacesCompleted { get; set; } = racesCompleted;
        public int Wins { get; set; } = wins;
    }
}
