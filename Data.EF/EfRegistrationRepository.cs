using IRacingLeague.Data;
using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public sealed class EfRegistrationRepository(AppDbContext context) : IRegistrationRepository
{
    public Task AddAsync(Registration registration, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Registrations.Add(registration); return Task.CompletedTask; }
    public Task<Registration?> GetAsync(int id, CancellationToken cancellationToken = default) => context.Registrations.FindAsync([id], cancellationToken).AsTask();
    public async Task<IReadOnlyList<Registration>> GetByLeagueAsync(int leagueId, CancellationToken cancellationToken = default) => await context.Registrations.Where(r => r.LeagueId == leagueId).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<Registration>> GetByUserAsync(int userId, CancellationToken cancellationToken = default) => await context.Registrations.Where(r => r.UserId == userId).ToListAsync(cancellationToken);
    public Task<Registration?> FindMembershipAsync(int userId, int leagueId, CancellationToken cancellationToken = default) => context.Registrations.FirstOrDefaultAsync(r => r.UserId == userId && r.LeagueId == leagueId, cancellationToken);
    public Task<bool> HasMembershipAsync(int userId, int leagueId, CancellationToken cancellationToken = default) => context.Registrations.AnyAsync(r => r.UserId == userId && r.LeagueId == leagueId, cancellationToken);
    public Task<int> CountByLeagueAsync(int leagueId, CancellationToken cancellationToken = default) => context.Registrations.CountAsync(r => r.LeagueId == leagueId, cancellationToken);
    public Task UpdateAsync(Registration registration, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Registrations.Update(registration); return Task.CompletedTask; }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    { var registration = await context.Registrations.FindAsync([id], cancellationToken); if (registration is not null) context.Registrations.Remove(registration); }
}
