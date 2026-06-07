using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public class EfRaceRepository : IRaceRepository
{
    private readonly AppDbContext _context;

    public EfRaceRepository(AppDbContext context) => _context = context;

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

    public void SaveChanges() => _context.SaveChanges();
}
