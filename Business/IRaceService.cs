using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;

public interface IRaceService
{
    Task<Race> CreateAsync(int leagueId, string track, string car, DateTime scheduledAt, int lapCount, decimal ambientTempC, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Race>> GetByLeagueAsync(int leagueId, RaceQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<Race> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Race race, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
