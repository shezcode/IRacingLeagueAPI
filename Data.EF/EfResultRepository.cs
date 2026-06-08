using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public class EfResultRepository : IResultRepository
{
    private readonly AppDbContext _context;

    public EfResultRepository(AppDbContext context) => _context = context;

    public void Add(Result result) => _context.Results.Add(result);

    public Result? Get(int id) => _context.Results.Find(id);

    public IEnumerable<Result> GetAll() => _context.Results.AsNoTracking().ToList();

    public void Update(Result result) => _context.Results.Update(result);

    public void Delete(int id)
    {
        var result = _context.Results.Find(id);
        if (result is not null)
            _context.Results.Remove(result);
    }

    public void SaveChanges() => _context.SaveChanges();
}
