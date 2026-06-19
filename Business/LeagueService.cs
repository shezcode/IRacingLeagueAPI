using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Business;

public class LeagueService : ILeagueService
{
    private readonly ILeagueRepository _repository;
    private readonly IRegistrationService _registrations;
    private readonly IRaceService _races;

    public LeagueService(ILeagueRepository repository, IRegistrationService registrations, IRaceService races)
    {
        _repository = repository;
        _registrations = registrations;
        _races = races;
    }

    public League Create(string name, string discipline, bool isPublic, int maxDrivers, decimal entryFee, int ownerUserId = 0)
    {
        var league = new League(name, discipline, isPublic, maxDrivers, entryFee, ownerUserId);
        _repository.Add(league);
        _repository.SaveChanges();
        return league;
    }

    public IEnumerable<League> GetAll() => _repository.GetAll();

    public IEnumerable<League> GetPublic() =>
        _repository.GetAll().Where(l => l.IsPublic).ToList();

    // Filter/sort applied over the repository's IQueryable so the predicates translate
    // to SQL and run in the database. The public-zone constraint (IsPublic) is pinned
    // here and never client-toggleable, so a private league can't leak through the list.
    public IEnumerable<League> GetPublic(LeagueQueryParameters p)
    {
        var q = _repository.Query().Where(l => l.IsPublic);

        if (!string.IsNullOrWhiteSpace(p.Discipline))
            q = q.Where(l => l.Discipline == p.Discipline);   // case-insensitive via DB collation

        q = p.SortBy?.ToLowerInvariant() switch
        {
            "entryfee" => p.Desc ? q.OrderByDescending(l => l.EntryFee) : q.OrderBy(l => l.EntryFee),
            "createdat" => p.Desc ? q.OrderByDescending(l => l.CreatedAt) : q.OrderBy(l => l.CreatedAt),
            "maxdrivers" => p.Desc ? q.OrderByDescending(l => l.MaxDrivers) : q.OrderBy(l => l.MaxDrivers),
            _ => p.Desc ? q.OrderByDescending(l => l.Name) : q.OrderBy(l => l.Name),
        };

        return q.ToList();
    }

    public IEnumerable<League> GetOwnedBy(int ownerUserId) =>
        _repository.GetAll().Where(l => l.OwnerUserId == ownerUserId).ToList();

    // Single-field search: leagues whose Name contains the term (case-insensitive).
    // Filtering happens here in the service layer, reusing the existing repository.
    public IEnumerable<League> SearchByName(string name) =>
        _repository.GetAll()
            .Where(l => l.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

    public League GetById(int id)
    {
        var league = _repository.Get(id);
        if (league == null)
            throw new KeyNotFoundException($"League with id {id} not found.");
        return league;
    }

    public void Update(League league)
    {
        if (_repository.Get(league.LeagueId) == null)
            throw new KeyNotFoundException($"League with id {league.LeagueId} not found.");
        _repository.Update(league);
        _repository.SaveChanges();
    }

    public void Delete(int id)
    {
        if (_repository.Get(id) == null)
            throw new KeyNotFoundException($"League with id {id} not found.");

        // Cascade through the lower-level deletes so each registration's and race's results
        // are removed (reversing their stat contributions) before the league itself goes.
        // Registrations first: that clears every result in the league, so the race deletes
        // below find none left. FKs are Restrict, so children must be gone before the parent.
        foreach (var registration in _registrations.GetByLeague(id).ToList())
            _registrations.Delete(registration.RegistrationId);

        foreach (var race in _races.GetByLeague(id).ToList())
            _races.Delete(race.RaceId);

        _repository.Delete(id);
        _repository.SaveChanges();
    }
}
