using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public class EfLeagueRepository : ILeagueRepository
{
    private readonly AppDbContext _context;

    public EfLeagueRepository(AppDbContext context) => _context = context;

    public void Add(League league) => _context.Leagues.Add(league);

    public League? Get(int id) => _context.Leagues.Find(id);

    public IEnumerable<League> GetAll() => _context.Leagues.AsNoTracking().ToList();

    public IQueryable<League> Query() => _context.Leagues.AsNoTracking();

    public void Update(League league) => _context.Leagues.Update(league);

    public void Delete(int id)
    {
        var league = _context.Leagues.Find(id);
        if (league is not null)
            _context.Leagues.Remove(league);
    }

    public void SaveChanges() => _context.SaveChanges();
}
