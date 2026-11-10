using IRacingLeague.Models.Dtos;
using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface IRegistrationRepository
{
    Task AddAsync(Registration registration, CancellationToken cancellationToken = default);
    Task<Registration?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetByLeagueAsync(int leagueId, CancellationToken cancellationToken = default);
    Task<PagedResponse<Registration>> GetByLeaguePageAsync(int leagueId, PaginationParameters parameters, CancellationToken cancellationToken = default);
    Task<PagedResponse<Registration>> GetStandingsPageAsync(int leagueId, PaginationParameters parameters, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<Registration?> FindMembershipAsync(int userId, int leagueId, CancellationToken cancellationToken = default);
    Task<bool> HasMembershipAsync(int userId, int leagueId, CancellationToken cancellationToken = default);
    Task<int> CountByLeagueAsync(int leagueId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Registration registration, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
