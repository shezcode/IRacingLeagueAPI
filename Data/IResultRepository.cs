using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface IResultRepository
{
    Task AddAsync(Result result, CancellationToken cancellationToken = default);
    Task<Result?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Result>> GetByRaceAsync(int raceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Result>> GetByRegistrationAsync(int registrationId, CancellationToken cancellationToken = default);
    Task<Result?> FindForRaceAndRegistrationAsync(int raceId, int registrationId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Result result, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
