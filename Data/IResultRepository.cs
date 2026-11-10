using IRacingLeague.Models.Dtos;
using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface IResultRepository
{
    Task AddAsync(Result result, CancellationToken cancellationToken = default);
    Task ReplaceRaceAsync(int raceId, IReadOnlyList<Result> results, CancellationToken cancellationToken = default);
    Task ClearRaceAsync(int raceId, CancellationToken cancellationToken = default);
    Task<Result?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Result>> GetByRaceAsync(int raceId, CancellationToken cancellationToken = default);
    Task<PagedResponse<Result>> GetByRacePageAsync(int raceId, PaginationParameters parameters, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Result>> GetByRegistrationAsync(int registrationId, CancellationToken cancellationToken = default);
    Task<Result?> FindForRaceAndRegistrationAsync(int raceId, int registrationId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Result result, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
