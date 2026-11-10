using IRacingLeague.Models;
namespace IRacingLeague.Business;

public interface IRegistrationService
{
    Task<Registration> RegisterAsync(int userId, int leagueId, int carNumber, string teamName, decimal ballastKg, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(int userId, int leagueId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetByLeagueAsync(int leagueId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<Registration> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
