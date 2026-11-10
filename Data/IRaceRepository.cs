using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Data;

public interface IRaceRepository
{
    Task AddAsync(Race race, CancellationToken cancellationToken = default);
    Task<Race?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Race>> GetByLeagueAsync(int leagueId, RaceQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Race>> GetScheduledByLeagueAsync(int leagueId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Race race, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
