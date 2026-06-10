using System.Security.Claims;
using IRacingLeague.Business;
using IRacingLeague.Models;

namespace IRacingLeague.API.Authorization;

public class LeagueViewPolicy
{
    private readonly IRegistrationService _registrations;
    private readonly IAuthService _auth;

    public LeagueViewPolicy(IRegistrationService registrations, IAuthService auth)
    {
        _registrations = registrations;
        _auth = auth;
    }

    public bool CanView(League league, ClaimsPrincipal user)
    {
        if (league.IsPublic)
            return true;
        if (_auth.HasAccessToResource(league.OwnerUserId, user))   // owner or Admin
            return true;

        var callerId = _auth.GetUserId(user);
        return callerId is not null &&
               _registrations.GetByLeague(league.LeagueId).Any(r => r.UserId == callerId);
    }
}
