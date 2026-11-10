using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface IRaceRepository
{
    Task AddAsync(Race race, CancellationToken cancellationToken = default);
    Task<Race?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Race>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Race race);
    Race? Get(int id);
    IEnumerable<Race> GetAll();
    IQueryable<Race> Query();
    void Update(Race race);
    void Delete(int id);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
