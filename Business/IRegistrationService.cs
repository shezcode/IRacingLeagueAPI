using IRacingLeague.Models.Dtos;
using IRacingLeague.Models;
namespace IRacingLeague.Business;

public interface IRegistrationService
{
    Task<Registration> RegisterAsync(int userId, int leagueId, int carNumber, string teamName, decimal ballastKg, string? inviteToken = null, bool requireInvite = true, CancellationToken cancellationToken = default);
    Task<(string Token, DateTime CreatedAt, DateTime ExpiresAt)> CreateInviteAsync(int leagueId, int creatorUserId, int expiresInHours, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeagueInvite>> GetInvitesAsync(int leagueId, CancellationToken cancellationToken = default);
    Task RevokeInviteAsync(int leagueId, int inviteId, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(int userId, int leagueId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetByLeagueAsync(int leagueId, CancellationToken cancellationToken = default);
    Task<PagedResponse<Registration>> GetByLeaguePageAsync(int leagueId, PaginationParameters parameters, CancellationToken cancellationToken = default);
    Task<PagedResponse<Registration>> GetStandingsPageAsync(int leagueId, PaginationParameters parameters, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<Registration> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
