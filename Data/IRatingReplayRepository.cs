using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface IRatingReplayRepository
{
    Task AcquireGlobalLockAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetUserIdsForRegistrationsAsync(IReadOnlyCollection<int> registrationIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RatingReplayEntry>> GetEntriesBeforeAsync(DateTime scheduledAt, int raceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RatingReplayEntry>> GetEntriesFromAsync(DateTime scheduledAt, int raceId, CancellationToken cancellationToken = default);
}
