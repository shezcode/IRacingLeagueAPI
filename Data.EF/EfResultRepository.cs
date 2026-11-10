using IRacingLeague.Data;
using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public sealed class EfResultRepository(AppDbContext context) : IResultRepository
{
    public Task AddAsync(Result result, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Results.Add(result); return Task.CompletedTask; }
    public Task<Result?> GetAsync(int id, CancellationToken cancellationToken = default) => context.Results.FindAsync([id], cancellationToken).AsTask();
    public async Task<IReadOnlyList<Result>> GetByRaceAsync(int raceId, CancellationToken cancellationToken = default) => await context.Results.Where(r => r.RaceId == raceId).OrderBy(r => r.Position).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<Result>> GetByRegistrationAsync(int registrationId, CancellationToken cancellationToken = default) => await context.Results.Where(r => r.RegistrationId == registrationId).ToListAsync(cancellationToken);
    public Task<Result?> FindForRaceAndRegistrationAsync(int raceId, int registrationId, CancellationToken cancellationToken = default) => context.Results.FirstOrDefaultAsync(r => r.RaceId == raceId && r.RegistrationId == registrationId, cancellationToken);
    public Task UpdateAsync(Result result, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); context.Results.Update(result); return Task.CompletedTask; }
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    { var result = await context.Results.FindAsync([id], cancellationToken); if (result is not null) context.Results.Remove(result); }
}
