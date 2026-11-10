using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public sealed class EfRaceRepository(AppDbContext context) : IRaceRepository
{
    public Task AddAsync(Race race, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Races.Add(race); return Task.CompletedTask; }
    public Task<Race?> GetAsync(int id, CancellationToken cancellationToken = default) => context.Races.FindAsync([id], cancellationToken).AsTask();
    public async Task<IReadOnlyList<Race>> GetByLeagueAsync(int leagueId, RaceQueryParameters p, CancellationToken cancellationToken = default)
    {
        var q = context.Races.AsNoTracking().Where(r => r.LeagueId == leagueId);
        if (!string.IsNullOrWhiteSpace(p.Track)) q = q.Where(r => r.Track.Contains(p.Track));
        if (p.ScheduledFrom.HasValue) q = q.Where(r => r.ScheduledAt >= AsUtc(p.ScheduledFrom.Value));
        if (p.ScheduledTo.HasValue) q = q.Where(r => r.ScheduledAt <= AsUtc(p.ScheduledTo.Value));
        q = p.SortBy?.ToLowerInvariant() switch
        {
            "scheduledat" => p.Desc ? q.OrderByDescending(r => r.ScheduledAt) : q.OrderBy(r => r.ScheduledAt),
            "track" => p.Desc ? q.OrderByDescending(r => r.Track) : q.OrderBy(r => r.Track),
            _ => p.Desc ? q.OrderByDescending(r => r.Round) : q.OrderBy(r => r.Round)
        };
        return await q.ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<Race>> GetScheduledByLeagueAsync(int leagueId, CancellationToken cancellationToken = default) =>
        await context.Races.Where(r => r.LeagueId == leagueId).OrderBy(r => r.ScheduledAt).ThenBy(r => r.RaceId).ToListAsync(cancellationToken);
    public Task UpdateAsync(Race race, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Races.Update(race); return Task.CompletedTask; }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    { var race = await context.Races.FindAsync([id], cancellationToken); if (race is not null) context.Races.Remove(race); }
    private static DateTime AsUtc(DateTime value) => value.Kind switch { DateTimeKind.Utc => value, DateTimeKind.Local => value.ToUniversalTime(), _ => DateTime.SpecifyKind(value, DateTimeKind.Utc) };
}
