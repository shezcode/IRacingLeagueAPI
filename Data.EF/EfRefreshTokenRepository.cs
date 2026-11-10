using IRacingLeague.Data;
using IRacingLeague.Models;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.Data.EF;

public sealed class EfRefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        context.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public Task<RefreshToken?> FindForUpdateByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        context.RefreshTokens.FromSqlInterpolated($"SELECT * FROM \"RefreshTokens\" WHERE \"TokenHash\" = {tokenHash} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

    public Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        context.RefreshTokens.Add(token);
        return Task.CompletedTask;
    }

    public async Task RevokeFamilyAsync(Guid familyId, DateTime revokedAt, CancellationToken cancellationToken = default) =>
        _ = await context.RefreshTokens.Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, revokedAt), cancellationToken);
}
