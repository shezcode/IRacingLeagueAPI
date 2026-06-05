using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Business;

public class RaceService : IRaceService
{
    private readonly IRaceRepository _races;
    private readonly ILeagueRepository _leagues;

    public RaceService(IRaceRepository races, ILeagueRepository leagues)
    {
        _races = races;
        _leagues = leagues;
    }

    public Race Create(int leagueId, string track, string car, DateTime scheduledAt, int lapCount, decimal ambientTempC, int round)
    {
        if (_leagues.Get(leagueId) == null)
            throw new KeyNotFoundException($"League with id {leagueId} not found.");

        var race = new Race(leagueId, track, car, scheduledAt, lapCount, ambientTempC, round);
        _races.Add(race);
        _races.SaveChanges();
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
    }

    public void Delete(int id)
    {
        if (_races.Get(id) == null)
            throw new KeyNotFoundException($"Race with id {id} not found.");
        _races.Delete(id);
        _races.SaveChanges();
    }
}
