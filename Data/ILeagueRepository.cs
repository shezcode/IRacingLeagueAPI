using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Data;

public interface ILeagueRepository
{
    Task AddAsync(League league, CancellationToken cancellationToken = default);
    Task<League?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<League>> GetAllAsync(CancellationToken cancellationToken = default);
    Task LockForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountMembersAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResponse<League>> GetPublicAsync(LeagueQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<PagedResponse<League>> GetForUserAsync(int userId, PaginationParameters parameters, CancellationToken cancellationToken = default);
    Task<int> CountOwnedByAsync(int userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(League league, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
