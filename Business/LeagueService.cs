using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;

namespace IRacingLeague.Business;

public class LeagueService : ILeagueService
{
    private readonly ILeagueRepository _repository;

    public LeagueService(ILeagueRepository repository)
    {
        _repository = repository;
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
        _repository.Delete(id);
        _repository.SaveChanges();
    }
}
