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
        try
        {
            var league = _leagues.GetById(leagueId);   // 404 if missing
            if (!_viewPolicy.CanView(league, User))
                return Forbid();

            var standings = _standings.GetStandings(leagueId)
                .Select(e => RegistrationReadDTO.FromEntity(e.Registration, e.RacesCompleted));
            return Ok(standings);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute standings for league {LeagueId}", leagueId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("leagues/{leagueId}/registrations")]
    [Authorize]
    public ActionResult<RegistrationReadDTO> Register(int leagueId, [FromBody] RegistrationCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var callerId = _auth.GetUserId(User);
        if (callerId is null)
            return Unauthorized();

        try
        {
            var targetUserId = dto.UserId ?? callerId.Value;

            // Registering a driver other than yourself is restricted to the league owner.
            if (targetUserId != callerId.Value)
            {
                var league = _leagues.GetById(leagueId);   // 404 if the league is missing
                if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                    return Forbid();
            }

            var registration = _service.Register(
                targetUserId, leagueId, dto.CarNumber, dto.TeamName, dto.BallastKg);

            return CreatedAtAction(
                nameof(GetStandings), new { leagueId }, RegistrationReadDTO.FromEntity(registration));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();   // the league does not exist
        }
        catch (DuplicateRegistrationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (LeagueFullException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register driver in league {LeagueId}", leagueId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("registrations/{id}")]
    [Authorize]
    public IActionResult Delete(int id)
    {
        try
        {
            var registration = _service.GetById(id);   // 404 if missing
            var league = _leagues.GetById(registration.LeagueId);

            var isSelf = _auth.GetUserId(User) == registration.UserId;
            var isOwnerOrAdmin = _auth.HasAccessToResource(league.OwnerUserId, User);
            if (!isSelf && !isOwnerOrAdmin)
                return Forbid();

            _service.Delete(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (DbUpdateException)
        {
            // Defensive fallback: the service cascades a membership's results before removing
            // it, so the Restrict FK shouldn't trip — but surface a 409 rather than a 500 if it does.
            return Conflict("Cannot remove a registration that has recorded results.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete registration {RegistrationId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
