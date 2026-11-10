using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface ILeagueRepository
{
    Task AddAsync(League league, CancellationToken cancellationToken = default);
    Task<League?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<League>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(League league);
    League? Get(int id);
    IEnumerable<League> GetAll();
    IQueryable<League> Query();
    void Update(League league);
    void Delete(int id);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
