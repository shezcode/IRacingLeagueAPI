using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface ILeagueInviteRepository
{
    Task AddAsync(LeagueInvite invite, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeagueInvite>> GetByLeagueAsync(int leagueId, CancellationToken cancellationToken = default);
    Task<LeagueInvite?> LockByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<LeagueInvite?> LockByIdAsync(int leagueId, int inviteId, CancellationToken cancellationToken = default);
}
