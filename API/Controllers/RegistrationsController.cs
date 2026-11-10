using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.API.Controllers;

[ApiController]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _service;
    private readonly IStandingsService _standings;
    private readonly ILeagueService _leagues;
    private readonly IAuthService _auth;
    private readonly LeagueViewPolicy _viewPolicy;
    private readonly ILogger<RegistrationsController> _logger;

    public RegistrationsController(
        IRegistrationService service, IStandingsService standings, ILeagueService leagues,
        IAuthService auth, LeagueViewPolicy viewPolicy, ILogger<RegistrationsController> logger)
    {
        _service = service;
        _standings = standings;
        _leagues = leagues;
        _auth = auth;
        _viewPolicy = viewPolicy;
        _logger = logger;
    }

    [HttpGet("leagues/{leagueId}/registrations")]
    public ActionResult<IEnumerable<RegistrationReadDTO>> GetStandings(int leagueId)
    {

            var league = _leagues.GetById(leagueId);   // 404 if missing
            if (!_viewPolicy.CanView(league, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            var standings = _standings.GetStandings(leagueId)
                .Select(e => RegistrationReadDTO.FromEntity(e.Registration, e.RacesCompleted));
            return Ok(standings);

    }

    [HttpPost("leagues/{leagueId}/registrations")]
    [Authorize]
    public ActionResult<RegistrationReadDTO> Register(int leagueId, [FromBody] RegistrationCreateDTO dto)
    {
        var callerId = _auth.GetUserId(User);
        if (callerId is null)
            throw new UnauthorizedAccessException("Authentication is required.");


            var targetUserId = dto.UserId ?? callerId.Value;

            // Registering a driver other than yourself is restricted to the league owner.
            if (targetUserId != callerId.Value)
            {
                var league = _leagues.GetById(leagueId);   // 404 if the league is missing
                if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                    throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");
            }

            var registration = _service.Register(
                targetUserId, leagueId, dto.CarNumber, dto.TeamName, dto.BallastKg);

            return CreatedAtAction(
                nameof(GetStandings), new { leagueId }, RegistrationReadDTO.FromEntity(registration));

    }

    [HttpDelete("registrations/{id}")]
    [Authorize]
    public IActionResult Delete(int id)
    {

            var registration = _service.GetById(id);   // 404 if missing
            var league = _leagues.GetById(registration.LeagueId);

            var isSelf = _auth.GetUserId(User) == registration.UserId;
            var isOwnerOrAdmin = _auth.HasAccessToResource(league.OwnerUserId, User);
            if (!isSelf && !isOwnerOrAdmin)
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            _service.Delete(id);
            return NoContent();

    }
}
