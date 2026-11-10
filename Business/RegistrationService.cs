using IRacingLeague.Data;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using System.Security.Cryptography;
using System.Text;
namespace IRacingLeague.Business;

public sealed class RegistrationService(IRegistrationRepository registrations, ILeagueRepository leagues, ILeagueInviteRepository invites, IRatingReplayRepository replayRepository, IRatingReplayService ratingReplay, IUnitOfWork unitOfWork) : IRegistrationService
{
    public async Task<Registration> RegisterAsync(int userId, int leagueId, int carNumber, string teamName, decimal ballastKg, string? inviteToken = null, bool requireInvite = true, CancellationToken cancellationToken = default)
    {
        Registration? registration = null;
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await leagues.LockForUpdateAsync(leagueId, ct);
            var league = await leagues.GetAsync(leagueId, ct) ?? throw new NotFoundException($"League with id {leagueId} not found.");
            if (!league.IsPublic && requireInvite)
            {
                var invite = string.IsNullOrWhiteSpace(inviteToken) ? null : await invites.LockByTokenHashAsync(HashToken(inviteToken), ct);
                if (invite is null || invite.LeagueId != leagueId || invite.ExpiresAt <= DateTime.UtcNow || invite.UsedAt is not null || invite.RevokedAt is not null)
                    throw new ForbiddenException("A valid invite for this private league is required.");
                invite.UsedAt = DateTime.UtcNow;
                invite.UsedByUserId = userId;
            }
            if (await registrations.FindMembershipAsync(userId, leagueId, ct) is not null) throw new ConflictException($"Driver #{userId} is already registered in league '{league.Name}'.");
            if (await registrations.CountByLeagueAsync(leagueId, ct) >= league.MaxDrivers) throw new ConflictException($"League '{league.Name}' is full ({league.MaxDrivers} drivers).");
            registration = new Registration(userId, leagueId, carNumber, teamName, ballastKg);
            await registrations.AddAsync(registration, ct);
        }, cancellationToken);
        return registration!;
    }
    public async Task<(string Token, DateTime CreatedAt, DateTime ExpiresAt)> CreateInviteAsync(int leagueId, int creatorUserId, int expiresInHours, CancellationToken cancellationToken = default)
    {
        if (expiresInHours is < 1 or > 168) throw new ArgumentOutOfRangeException(nameof(expiresInHours), "Invite expiry must be between 1 and 168 hours.");
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var now = DateTime.UtcNow;
        var invite = new LeagueInvite { LeagueId = leagueId, TokenHash = HashToken(token), CreatedByUserId = creatorUserId, CreatedAt = now, ExpiresAt = now.AddHours(expiresInHours) };
        await unitOfWork.ExecuteInTransactionAsync(async ct => { if (await leagues.GetAsync(leagueId, ct) is null) throw new NotFoundException($"League with id {leagueId} not found."); await invites.AddAsync(invite, ct); }, cancellationToken);
        return (token, invite.CreatedAt, invite.ExpiresAt);
    }
    public Task<IReadOnlyList<LeagueInvite>> GetInvitesAsync(int leagueId, CancellationToken cancellationToken = default) => invites.GetByLeagueAsync(leagueId, cancellationToken);
    public async Task RevokeInviteAsync(int leagueId, int inviteId, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var invite = await invites.LockByIdAsync(leagueId, inviteId, ct) ?? throw new NotFoundException($"Invite {inviteId} was not found in league {leagueId}.");
            if (invite.RevokedAt is null) invite.RevokedAt = DateTime.UtcNow;
        }, cancellationToken);
    }
    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    public Task<bool> IsMemberAsync(int userId, int leagueId, CancellationToken cancellationToken = default) => registrations.HasMembershipAsync(userId, leagueId, cancellationToken);
    public Task<IReadOnlyList<Registration>> GetByLeagueAsync(int leagueId, CancellationToken cancellationToken = default) => registrations.GetByLeagueAsync(leagueId, cancellationToken);
    public Task<PagedResponse<Registration>> GetByLeaguePageAsync(int leagueId, PaginationParameters parameters, CancellationToken cancellationToken = default) => registrations.GetByLeaguePageAsync(leagueId, parameters, cancellationToken);
    public Task<PagedResponse<Registration>> GetStandingsPageAsync(int leagueId, PaginationParameters parameters, CancellationToken cancellationToken = default) => registrations.GetStandingsPageAsync(leagueId, parameters, cancellationToken);
    public Task<IReadOnlyList<Registration>> GetByUserAsync(int userId, CancellationToken cancellationToken = default) => registrations.GetByUserAsync(userId, cancellationToken);
    public async Task<Registration> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await registrations.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"Registration with id {id} not found.");
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var registration = await registrations.GetAsync(id, cancellationToken) ?? throw new NotFoundException($"Registration with id {id} not found.");
        var firstAffectedRace = await replayRepository.GetEarliestRaceForRegistrationsAsync([id], cancellationToken);
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await registrations.DeleteAsync(id, ct);
            await unitOfWork.SaveChangesAsync(ct);
            if (firstAffectedRace is not null)
                await ratingReplay.ReplayFromAsync(firstAffectedRace.ScheduledAt, firstAffectedRace.RaceId, [id], ct, [registration.UserId]);
        }, cancellationToken);
    }
}
