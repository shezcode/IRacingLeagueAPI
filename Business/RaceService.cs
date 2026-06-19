using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Business;

public class RaceService : IRaceService
{
    private readonly IRaceRepository _races;
    private readonly ILeagueRepository _leagues;
    private readonly IResultService _results;

    public RaceService(IRaceRepository races, ILeagueRepository leagues, IResultService results)
    {
        _races = races;
        _leagues = leagues;
        _results = results;
    }

    public Race Create(int leagueId, string track, string car, DateTime scheduledAt, int lapCount, decimal ambientTempC)
    {
        if (_leagues.Get(leagueId) == null)
            throw new KeyNotFoundException($"League with id {leagueId} not found.");

        var race = new Race(leagueId, track, car, scheduledAt, lapCount, ambientTempC);
        _races.Add(race);
        _races.SaveChanges();       // populates race.RaceId

        RenumberRounds(leagueId);   // assign Round from schedule order, including the new race
        return race;
    }

    public IEnumerable<Race> GetByLeague(int leagueId) =>
        _races.GetAll().Where(r => r.LeagueId == leagueId).OrderBy(r => r.Round).ToList();

    // Filter (Track contains, ScheduledAt range) + sort applied over IQueryable in SQL.
    public IEnumerable<Race> GetByLeague(int leagueId, RaceQueryParameters p)
    {
        var q = _races.Query().Where(r => r.LeagueId == leagueId);

        if (!string.IsNullOrWhiteSpace(p.Track))
            q = q.Where(r => r.Track.Contains(p.Track));   // case-insensitive via DB collation
        if (p.ScheduledFrom.HasValue)
            q = q.Where(r => r.ScheduledAt >= p.ScheduledFrom.Value);
        if (p.ScheduledTo.HasValue)
            q = q.Where(r => r.ScheduledAt <= p.ScheduledTo.Value);

        q = p.SortBy?.ToLowerInvariant() switch
        {
            "scheduledat" => p.Desc ? q.OrderByDescending(r => r.ScheduledAt) : q.OrderBy(r => r.ScheduledAt),
            "track"       => p.Desc ? q.OrderByDescending(r => r.Track)       : q.OrderBy(r => r.Track),
            _             => p.Desc ? q.OrderByDescending(r => r.Round)       : q.OrderBy(r => r.Round),
        };

        return q.ToList();
    }

    public Race GetById(int id)
    {
        var race = _races.Get(id);
        if (race == null)
            throw new KeyNotFoundException($"Race with id {id} not found.");
        return race;
    }

    public void Update(Race race)
    {
        if (_races.Get(race.RaceId) == null)
            throw new KeyNotFoundException($"Race with id {race.RaceId} not found.");
        _races.Update(race);
        _races.SaveChanges();

        // ScheduledAt may have changed, which reorders the league's schedule, so
        // re-derive rounds. Idempotent when the order is unchanged.
        RenumberRounds(race.LeagueId);
    }

    public void Delete(int id)
    {
        var race = _races.Get(id);
        if (race == null)
            throw new KeyNotFoundException($"Race with id {id} not found.");

        var leagueId = race.LeagueId;

        // Clear recorded results first (reversing their stat contributions); the FK is
        // Restrict, so the race row can only be removed once it has no results.
        _results.DeleteForRace(id);

        _races.Delete(id);
        _races.SaveChanges();

        RenumberRounds(leagueId);   // close the gap left by the removed race
    }

    // Rounds are derived, never user-entered: order a league's races by ScheduledAt
    // (ties broken by RaceId) and assign Round = 1, 2, 3, ... contiguously.
    // Runs after every create/delete so Round stays contiguous and unique per league.
    private void RenumberRounds(int leagueId)
    {
        var orderedIds = _races.GetAll()
            .Where(r => r.LeagueId == leagueId)
            .OrderBy(r => r.ScheduledAt)
            .ThenBy(r => r.RaceId)
            .Select(r => r.RaceId)
            .ToList();

        var round = 1;
        foreach (var raceId in orderedIds)
        {
            // Fetch tracked (Find) so the just-added race resolves to the same instance
            // rather than a second copy that would collide on its key.
            var race = _races.Get(raceId);
            if (race is not null && race.Round != round)
            {
                race.Round = round;
                _races.Update(race);
            }
            round++;
        }

        _races.SaveChanges();
    }
}
