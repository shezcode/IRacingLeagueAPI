using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public sealed class EfLeagueRepository(AppDbContext context) : ILeagueRepository
{
    public Task AddAsync(League league, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Leagues.Add(league); return Task.CompletedTask; }
    public Task<League?> GetAsync(int id, CancellationToken cancellationToken = default) => context.Leagues.FindAsync([id], cancellationToken).AsTask();
    public async Task<PagedResponse<League>> GetPublicAsync(LeagueQueryParameters p, CancellationToken cancellationToken = default)
    {
        var q = context.Leagues.AsNoTracking().Where(l => l.IsPublic);
        if (!string.IsNullOrWhiteSpace(p.Discipline)) q = q.Where(l => l.Discipline.ToLower() == p.Discipline.ToLower());
        q = p.SortBy?.ToLowerInvariant() switch
        {
            "entryfee" => p.Desc ? q.OrderByDescending(l => l.EntryFee) : q.OrderBy(l => l.EntryFee),
            "createdat" => p.Desc ? q.OrderByDescending(l => l.CreatedAt) : q.OrderBy(l => l.CreatedAt),
            "maxdrivers" => p.Desc ? q.OrderByDescending(l => l.MaxDrivers) : q.OrderBy(l => l.MaxDrivers),
            _ => p.Desc ? q.OrderByDescending(l => l.Name) : q.OrderBy(l => l.Name)
        };
        var totalCount = await q.CountAsync(cancellationToken);
        var items = await q.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync(cancellationToken);
        return new PagedResponse<League>(items, p.Page, p.PageSize, totalCount);
    }
    public async Task<PagedResponse<League>> GetForUserAsync(int userId, PaginationParameters p, CancellationToken cancellationToken = default)
    {
        var query = context.Leagues.AsNoTracking()
            .Where(l => l.OwnerUserId == userId || context.Registrations.Any(r => r.LeagueId == l.LeagueId && r.UserId == userId))
            .OrderBy(l => l.LeagueId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync(cancellationToken);
        return new PagedResponse<League>(items, p.Page, p.PageSize, totalCount);
    }
    public Task<int> CountOwnedByAsync(int userId, CancellationToken cancellationToken = default) => context.Leagues.CountAsync(l => l.OwnerUserId == userId, cancellationToken);
    public Task UpdateAsync(League league, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Leagues.Update(league); return Task.CompletedTask; }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    { var league = await context.Leagues.FindAsync([id], cancellationToken); if (league is not null) context.Leagues.Remove(league); }
}
