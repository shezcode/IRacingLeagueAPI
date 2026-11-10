using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public sealed class EfUserRepository(AppDbContext context) : IUserRepository
{
    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Users.Add(user); return Task.CompletedTask; }
    public async Task<User?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Users.FindAsync([id], cancellationToken);
    }
    public async Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) =>
        await context.Users.Where(u => ids.Contains(u.UserId)).ToListAsync(cancellationToken);
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) => context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) => context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    public async Task<PagedResponse<User>> QueryAsync(UserQueryParameters p, CancellationToken cancellationToken = default)
    {
        var q = context.Users.AsNoTracking().AsQueryable();
        if (p.MinIRating.HasValue) q = q.Where(u => u.IRating >= p.MinIRating.Value);
        if (!string.IsNullOrWhiteSpace(p.MinLicenseClass)) { var order = new[] { "Rookie", "D", "C", "B", "A" }; var min = Array.FindIndex(order, c => c.Equals(p.MinLicenseClass, StringComparison.OrdinalIgnoreCase)); if (min >= 0) q = q.Where(u => order.Skip(min).Contains(u.LicenseClass)); }
        if (!string.IsNullOrWhiteSpace(p.LicenseClass)) q = q.Where(u => u.LicenseClass == p.LicenseClass);
        q = p.SortBy?.ToLowerInvariant() switch
        {
            "wins" or "totalwins" => p.Desc ? q.OrderByDescending(u => u.TotalWins) : q.OrderBy(u => u.TotalWins),
            "username" => p.Desc ? q.OrderByDescending(u => u.UserName) : q.OrderBy(u => u.UserName),
            _ => p.Desc ? q.OrderByDescending(u => u.IRating) : q.OrderBy(u => u.IRating)
        };
        var totalCount = await q.CountAsync(cancellationToken);
        var items = await q.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync(cancellationToken);
        return new PagedResponse<User>(items, p.Page, p.PageSize, totalCount);
    }
    public async Task<DriverStatsDTO> GetDriverStatsAsync(int userId, int? viewerUserId, bool canViewAllPrivate, CancellationToken cancellationToken = default)
    {
        var visibleResults = from result in context.Results.AsNoTracking()
                             join registration in context.Registrations.AsNoTracking() on result.RegistrationId equals registration.RegistrationId
                             join race in context.Races.AsNoTracking() on result.RaceId equals race.RaceId
                             join league in context.Leagues.AsNoTracking() on race.LeagueId equals league.LeagueId
                             where registration.UserId == userId &&
                                 (league.IsPublic || canViewAllPrivate || (viewerUserId.HasValue &&
                                     (league.OwnerUserId == viewerUserId.Value || context.Registrations.Any(member => member.LeagueId == league.LeagueId && member.UserId == viewerUserId.Value))))
                             select new { Result = result, Race = race, League = league };

        var totals = await visibleResults.GroupBy(_ => 1).Select(group => new TotalsSqlRow
        {
            Races = group.Count(),
            Wins = group.Count(x => x.Result.Position == 1 && !x.Result.Dnf),
            Podiums = group.Count(x => x.Result.Position <= 3 && !x.Result.Dnf),
            Dnfs = group.Count(x => x.Result.Dnf),
            AverageFinish = group.Average(x => (decimal)x.Result.Position),
            AverageIncidents = group.Average(x => (decimal)x.Result.IncidentPoints),
            FastestLaps = group.Count(x => x.Result.FastestLap)
        }).SingleOrDefaultAsync(cancellationToken);

        var historyRows = await visibleResults.OrderBy(x => x.Race.ScheduledAt).ThenBy(x => x.Race.RaceId)
            .Select(x => new HistorySqlRow
            {
                RaceId = x.Race.RaceId,
                LeagueId = x.League.LeagueId,
                LeagueName = x.League.Name,
                Track = x.Race.Track,
                Date = x.Race.ScheduledAt,
                IRatingBefore = x.Result.IRatingBefore,
                IRatingChange = x.Result.IRatingChange,
                SafetyRatingBefore = x.Result.SafetyRatingBefore,
                SafetyRatingChange = x.Result.SafetyRatingChange
            }).ToListAsync(cancellationToken);

        var irating = historyRows.Select(x => new RatingHistoryPoint(x.RaceId, x.LeagueId, x.LeagueName, x.Track, x.Date,
            x.IRatingBefore + x.IRatingChange, x.IRatingChange)).ToArray();
        var safety = historyRows.Select(x => new RatingHistoryPoint(x.RaceId, x.LeagueId, x.LeagueName, x.Track, x.Date,
            x.SafetyRatingBefore + x.SafetyRatingChange, x.SafetyRatingChange)).ToArray();
        return new DriverStatsDTO(totals?.Races ?? 0, totals?.Wins ?? 0, totals?.Podiums ?? 0, totals?.Dnfs ?? 0,
            totals?.AverageFinish, totals?.AverageIncidents, totals?.FastestLaps ?? 0, irating, safety);
    }
    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Users.Update(user); return Task.CompletedTask; }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    { var user = await context.Users.FindAsync([id], cancellationToken); if (user is not null) context.Users.Remove(user); }

    private sealed class TotalsSqlRow
    {
        public int Races { get; set; }
        public int Wins { get; set; }
        public int Podiums { get; set; }
        public int Dnfs { get; set; }
        public decimal AverageFinish { get; set; }
        public decimal AverageIncidents { get; set; }
        public int FastestLaps { get; set; }
    }

    private sealed class HistorySqlRow
    {
        public int RaceId { get; set; }
        public int LeagueId { get; set; }
        public string LeagueName { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int IRatingBefore { get; set; }
        public int IRatingChange { get; set; }
        public decimal SafetyRatingBefore { get; set; }
        public decimal SafetyRatingChange { get; set; }
    }
}
