using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public EfUserRepository(AppDbContext context) => _context = context;

    public void Add(User user) => _context.Users.Add(user);

    public User? Get(int id) => _context.Users.Find(id);

    public IEnumerable<User> GetAll() => _context.Users.AsNoTracking().ToList();

    public IQueryable<User> Query() => _context.Users.AsNoTracking();

    public void Update(User user) => _context.Users.Update(user);

    public void Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user is not null)
            _context.Users.Remove(user);
    }

    public void SaveChanges() => _context.SaveChanges();
}
