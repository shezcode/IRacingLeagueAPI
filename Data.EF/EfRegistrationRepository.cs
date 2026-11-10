using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public class EfRegistrationRepository : IRegistrationRepository
{
    private readonly AppDbContext _context;

    public EfRegistrationRepository(AppDbContext context) => _context = context;

    public Task AddAsync(Registration registration, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.Registrations.Add(registration);
        return Task.CompletedTask;
    }

    public Task<Registration?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Registrations.FindAsync(new object[] { id }, cancellationToken).AsTask();

    public async Task<IReadOnlyList<Registration>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Registrations.AsNoTracking().ToListAsync(cancellationToken);

    public void Add(Registration registration) => _context.Registrations.Add(registration);

    public Registration? Get(int id) => _context.Registrations.Find(id);

    public IEnumerable<Registration> GetAll() => _context.Registrations.AsNoTracking().ToList();

    public void Update(Registration registration) => _context.Registrations.Update(registration);

    public void Delete(int id)
    {
        var registration = _context.Registrations.Find(id);
        if (registration is not null)
            _context.Registrations.Remove(registration);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
    public async Task<IReadOnlyList<Registration>> GetByLeagueAsync(int leagueId, CancellationToken cancellationToken = default) =>
        await _context.Registrations.Where(r => r.LeagueId == leagueId).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<Registration>> GetByUserAsync(int userId, CancellationToken cancellationToken = default) =>
        await _context.Registrations.Where(r => r.UserId == userId).ToListAsync(cancellationToken);
    public Task<Registration?> FindMembershipAsync(int userId, int leagueId, CancellationToken cancellationToken = default) =>
        _context.Registrations.FirstOrDefaultAsync(r => r.UserId == userId && r.LeagueId == leagueId, cancellationToken);
    public Task<int> CountByLeagueAsync(int leagueId, CancellationToken cancellationToken = default) =>
        _context.Registrations.CountAsync(r => r.LeagueId == leagueId, cancellationToken);
}
