using IRacingLeague.Models.Dtos;
using IRacingLeague.Models;
namespace IRacingLeague.Business;

public interface IResultService
{
    Task<Result> ApplyResultAsync(Registration registration, Race race, Result result, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Result>> GetByRaceAsync(int raceId, CancellationToken cancellationToken = default);
    Task<PagedResponse<Result>> GetByRacePageAsync(int raceId, PaginationParameters parameters, CancellationToken cancellationToken = default);
    Task<Result> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteForRaceAsync(int raceId, CancellationToken cancellationToken = default);
    Task DeleteForRegistrationAsync(int registrationId, CancellationToken cancellationToken = default);
}
