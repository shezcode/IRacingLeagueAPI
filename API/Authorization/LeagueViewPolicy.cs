using System.Security.Claims;
using IRacingLeague.Business;
using IRacingLeague.Models;
namespace IRacingLeague.API.Authorization;

public sealed class LeagueViewPolicy(IRegistrationService registrations, IAuthService auth)
{
    public async Task<bool> CanViewAsync(League league, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        if (league.IsPublic || auth.HasAccessToResource(league.OwnerUserId, user)) return true;
        var callerId = auth.GetUserId(user);
        return callerId is not null && await registrations.IsMemberAsync(callerId.Value, league.LeagueId, cancellationToken);
    }
}
