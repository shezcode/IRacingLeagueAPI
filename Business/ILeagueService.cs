using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
namespace IRacingLeague.Business;

public interface ILeagueService
{
    Task<League> CreateAsync(string name, string discipline, bool isPublic, int maxDrivers, decimal entryFee, int ownerUserId = 0, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<League>> GetPublicAsync(LeagueQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<League> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(League league, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
