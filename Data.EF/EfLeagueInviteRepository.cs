using IRacingLeague.Data;
using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public sealed class EfLeagueInviteRepository(AppDbContext context) : ILeagueInviteRepository
{
    public Task AddAsync(LeagueInvite invite, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.LeagueInvites.Add(invite); return Task.CompletedTask; }

    public async Task<IReadOnlyList<LeagueInvite>> GetByLeagueAsync(int leagueId, CancellationToken cancellationToken = default) =>
        await context.LeagueInvites.AsNoTracking().Where(x => x.LeagueId == leagueId).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public Task<LeagueInvite?> LockByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        context.LeagueInvites.FromSqlInterpolated($"SELECT * FROM \"LeagueInvites\" WHERE \"TokenHash\" = {tokenHash} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);

    public Task<LeagueInvite?> LockByIdAsync(int leagueId, int inviteId, CancellationToken cancellationToken = default) =>
        context.LeagueInvites.FromSqlInterpolated($"SELECT * FROM \"LeagueInvites\" WHERE \"LeagueId\" = {leagueId} AND \"LeagueInviteId\" = {inviteId} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
}
