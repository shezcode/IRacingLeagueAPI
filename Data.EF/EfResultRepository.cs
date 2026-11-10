using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public class EfResultRepository : IResultRepository
{
    private readonly AppDbContext _context;

    public EfResultRepository(AppDbContext context) => _context = context;

    public Task AddAsync(Result result, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.Results.Add(result);
        return Task.CompletedTask;
    }

    public Task<Result?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Results.FindAsync(new object[] { id }, cancellationToken).AsTask();

    public async Task<IReadOnlyList<Result>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Results.AsNoTracking().ToListAsync(cancellationToken);

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

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
    public async Task<IReadOnlyList<Result>> GetByRaceAsync(int raceId, CancellationToken cancellationToken = default) =>
        await _context.Results.Where(r => r.RaceId == raceId).OrderBy(r => r.Position).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<Result>> GetByRegistrationAsync(int registrationId, CancellationToken cancellationToken = default) =>
        await _context.Results.Where(r => r.RegistrationId == registrationId).ToListAsync(cancellationToken);
    public Task<Result?> FindForRaceAndRegistrationAsync(int raceId, int registrationId, CancellationToken cancellationToken = default) =>
        _context.Results.FirstOrDefaultAsync(r => r.RaceId == raceId && r.RegistrationId == registrationId, cancellationToken);
}
