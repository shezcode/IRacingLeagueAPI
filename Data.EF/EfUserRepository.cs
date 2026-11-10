using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public EfUserRepository(AppDbContext context) => _context = context;

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task<User?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Users.FindAsync(new object[] { id }, cancellationToken).AsTask();

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Users.AsNoTracking().ToListAsync(cancellationToken);

    public void Add(User user) => _context.Users.Add(user);

    public User? Get(int id) => _context.Users.Find(id);

    public IEnumerable<User> GetAll() => _context.Users.AsNoTracking().ToList();

    public IQueryable<User> Query() => _context.Users.AsNoTracking();
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);

    public void Update(User user) => _context.Users.Update(user);

    public void Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user is not null)
            _context.Users.Remove(user);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
