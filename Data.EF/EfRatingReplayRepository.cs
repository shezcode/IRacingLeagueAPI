using IRacingLeague.Data;
using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public sealed class EfRatingReplayRepository(AppDbContext context) : IRatingReplayRepository
{
    public async Task AcquireGlobalLockAsync(CancellationToken cancellationToken = default) =>
        await context.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(2147483000, 15)", cancellationToken);

    public async Task<IReadOnlyList<int>> GetUserIdsForRegistrationsAsync(IReadOnlyCollection<int> registrationIds, CancellationToken cancellationToken = default)
    {
        if (registrationIds.Count == 0) return [];
        return await context.Registrations.AsNoTracking().Where(x => registrationIds.Contains(x.RegistrationId))
            .Select(x => x.UserId).Distinct().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RatingReplayEntry>> GetEntriesBeforeAsync(DateTime scheduledAt, int raceId, CancellationToken cancellationToken = default)
    {
        var query = from result in context.Results
                    join registration in context.Registrations on result.RegistrationId equals registration.RegistrationId
                    join race in context.Races on result.RaceId equals race.RaceId
                    where race.ScheduledAt < scheduledAt || (race.ScheduledAt == scheduledAt && race.RaceId < raceId)
                    orderby race.ScheduledAt, race.RaceId, result.Position
                    select new { Result = result, registration.UserId, race.ScheduledAt };
        var rows = await query.ToListAsync(cancellationToken);
        return rows.Select(x => new RatingReplayEntry(x.Result, x.UserId, x.ScheduledAt)).ToArray();
    }

    public async Task<IReadOnlyList<RatingReplayEntry>> GetEntriesFromAsync(DateTime scheduledAt, int raceId, CancellationToken cancellationToken = default)
    {
        var query = from result in context.Results
                    join registration in context.Registrations on result.RegistrationId equals registration.RegistrationId
                    join race in context.Races on result.RaceId equals race.RaceId
                    where race.ScheduledAt > scheduledAt || (race.ScheduledAt == scheduledAt && race.RaceId >= raceId)
                    orderby race.ScheduledAt, race.RaceId, result.Position
                    select new { Result = result, registration.UserId, race.ScheduledAt };
        var rows = await query.ToListAsync(cancellationToken);
        return rows.Select(x => new RatingReplayEntry(x.Result, x.UserId, x.ScheduledAt)).ToArray();
    }
}
