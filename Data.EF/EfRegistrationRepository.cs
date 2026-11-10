using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public sealed class EfRegistrationRepository(AppDbContext context) : IRegistrationRepository
{
    public Task AddAsync(Registration registration, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Registrations.Add(registration); return Task.CompletedTask; }
    public Task<Registration?> GetAsync(int id, CancellationToken cancellationToken = default) => context.Registrations.FindAsync([id], cancellationToken).AsTask();
    public async Task<IReadOnlyList<Registration>> GetByLeagueAsync(int leagueId, CancellationToken cancellationToken = default) => await context.Registrations.Where(r => r.LeagueId == leagueId).ToListAsync(cancellationToken);
    public async Task<PagedResponse<Registration>> GetByLeaguePageAsync(int leagueId, PaginationParameters p, CancellationToken cancellationToken = default)
    {
        var query = context.Registrations.AsNoTracking().Where(r => r.LeagueId == leagueId).OrderBy(r => r.RegistrationId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync(cancellationToken);
        return new PagedResponse<Registration>(items, p.Page, p.PageSize, totalCount);
    }
    public async Task<PagedResponse<Registration>> GetStandingsPageAsync(int leagueId, PaginationParameters p, CancellationToken cancellationToken = default)
    {
        var query = context.Registrations.AsNoTracking().Where(r => r.LeagueId == leagueId).OrderBy(r => r.RegistrationId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync(cancellationToken);
        return new PagedResponse<Registration>(items, p.Page, p.PageSize, totalCount);
    }
    public async Task<PagedResponse<StandingSummary>> GetStandingsAsync(int leagueId, PaginationParameters p, CancellationToken cancellationToken = default)
    {
        var total = await context.Registrations.CountAsync(x => x.LeagueId == leagueId, cancellationToken);
        var query = context.Database.SqlQuery<StandingSqlRow>($"""
            SELECT r."RegistrationId" AS "RegistrationId",
                   r."UserId" AS "UserId",
                   r."LeagueId" AS "LeagueId",
                   r."CarNumber" AS "CarNumber",
                   r."TeamName" AS "TeamName",
                   r."BallastKg" AS "BallastKg",
                   r."IsActive" AS "IsActive",
                   r."JoinedLeagueAt" AS "JoinedLeagueAt",
                   COUNT(result."ResultId")::integer AS "RacesCompleted",
                   COALESCE(SUM(result."Points"), 0)::integer AS "Points",
                   COUNT(result."ResultId") FILTER (WHERE result."Position" = 1 AND NOT result."Dnf")::integer AS "Wins",
                   COUNT(result."ResultId") FILTER (WHERE result."Position" <= 3 AND NOT result."Dnf")::integer AS "Podiums",
                   COALESCE(SUM(result."IncidentPoints"), 0)::integer AS "IncidentPoints",
                   COALESCE(MIN(result."Position"), 0)::integer AS "BestFinish"
            FROM "Registrations" AS r
            LEFT JOIN "Results" AS result ON result."RegistrationId" = r."RegistrationId"
            WHERE r."LeagueId" = {leagueId}
            GROUP BY r."RegistrationId"
            """);
        var rows = await query.OrderByDescending(x => x.Points).ThenByDescending(x => x.Wins).ThenBy(x => x.IncidentPoints).ThenBy(x => x.RegistrationId)
            .Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync(cancellationToken);
        var items = rows.Select(x => new StandingSummary(new Registration
        {
            RegistrationId = x.RegistrationId,
            UserId = x.UserId,
            LeagueId = x.LeagueId,
            CarNumber = x.CarNumber,
            TeamName = x.TeamName,
            BallastKg = x.BallastKg,
            IsActive = x.IsActive,
            JoinedLeagueAt = x.JoinedLeagueAt
        }, x.RacesCompleted, x.Points, x.Wins, x.Podiums, x.IncidentPoints, x.BestFinish)).ToArray();
        return new PagedResponse<StandingSummary>(items, p.Page, p.PageSize, total);
    }
    public async Task<IReadOnlyList<Registration>> GetByUserAsync(int userId, CancellationToken cancellationToken = default) => await context.Registrations.Where(r => r.UserId == userId).ToListAsync(cancellationToken);
    public Task<Registration?> FindMembershipAsync(int userId, int leagueId, CancellationToken cancellationToken = default) => context.Registrations.FirstOrDefaultAsync(r => r.UserId == userId && r.LeagueId == leagueId, cancellationToken);
    public Task<bool> HasMembershipAsync(int userId, int leagueId, CancellationToken cancellationToken = default) => context.Registrations.AnyAsync(r => r.UserId == userId && r.LeagueId == leagueId, cancellationToken);
    public Task<int> CountByLeagueAsync(int leagueId, CancellationToken cancellationToken = default) => context.Registrations.CountAsync(r => r.LeagueId == leagueId, cancellationToken);
    public Task UpdateAsync(Registration registration, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Registrations.Update(registration); return Task.CompletedTask; }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    { var registration = await context.Registrations.FindAsync([id], cancellationToken); if (registration is not null) context.Registrations.Remove(registration); }

    private sealed class StandingSqlRow
    {
        public int RegistrationId { get; set; }
        public int UserId { get; set; }
        public int LeagueId { get; set; }
        public int CarNumber { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public decimal BallastKg { get; set; }
        public bool IsActive { get; set; }
        public DateTime JoinedLeagueAt { get; set; }
        public int RacesCompleted { get; set; }
        public int Points { get; set; }
        public int Wins { get; set; }
        public int Podiums { get; set; }
        public int IncidentPoints { get; set; }
        public int BestFinish { get; set; }
    }
}
