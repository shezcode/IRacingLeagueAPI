using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public class EfRaceRepository : IRaceRepository
{
    private readonly AppDbContext _context;

    public EfRaceRepository(AppDbContext context) => _context = context;

    public Task AddAsync(Race race, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.Races.Add(race);
        return Task.CompletedTask;
    }

    public Task<Race?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Races.FindAsync(new object[] { id }, cancellationToken).AsTask();

    public async Task<IReadOnlyList<Race>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Races.AsNoTracking().ToListAsync(cancellationToken);

    public void Add(Race race) => _context.Races.Add(race);

    public Race? Get(int id) => _context.Races.Find(id);

    public IEnumerable<Race> GetAll() => _context.Races.AsNoTracking().ToList();

    public IQueryable<Race> Query() => _context.Races.AsNoTracking();

    public void Update(Race race) => _context.Races.Update(race);

    public void Delete(int id)
    {
        var race = _context.Races.Find(id);
        if (race is not null)
            _context.Races.Remove(race);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
