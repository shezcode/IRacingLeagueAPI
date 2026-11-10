using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Data;

public interface ILeagueRepository
{
    Task AddAsync(League league, CancellationToken cancellationToken = default);
    Task<League?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<League>> GetPublicAsync(LeagueQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<int> CountOwnedByAsync(int userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(League league, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
