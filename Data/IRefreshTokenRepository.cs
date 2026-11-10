using IRacingLeague.Models;

namespace IRacingLeague.Data;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<RefreshToken?> FindForUpdateByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task RevokeFamilyAsync(Guid familyId, DateTime revokedAt, CancellationToken cancellationToken = default);
}
