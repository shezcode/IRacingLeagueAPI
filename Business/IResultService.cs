using IRacingLeague.Models.Dtos;
using IRacingLeague.Models;
namespace IRacingLeague.Business;

public interface IResultService
{
    Task ReplaceClassificationAsync(int raceId, IReadOnlyList<IRacingLeague.Models.Dtos.ClassificationEntryDTO> entries, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Result>> PreviewClassificationAsync(int raceId, IReadOnlyList<ClassificationEntryDTO> entries, CancellationToken cancellationToken = default);
    Task ClearClassificationAsync(int raceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Result>> GetByRaceAsync(int raceId, CancellationToken cancellationToken = default);
    Task<PagedResponse<Result>> GetByRacePageAsync(int raceId, PaginationParameters parameters, CancellationToken cancellationToken = default);
    Task<Result> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
