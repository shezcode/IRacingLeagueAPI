using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace IRacingLeague.API.Controllers;

[ApiController]
public sealed class RegistrationsController(IRegistrationService service, IStandingsService standings, ILeagueService leagues, IAuthService auth, LeagueViewPolicy viewPolicy) : ControllerBase
{
    [HttpGet("api/v1/leagues/{leagueId}/members")]
    public async Task<ActionResult<IEnumerable<RegistrationReadDTO>>> GetStandings(int leagueId, CancellationToken ct)
    { var league = await leagues.GetByIdAsync(leagueId, ct); if (!await viewPolicy.CanViewAsync(league, User, ct)) throw new ForbiddenException("You do not have access to this resource."); return Ok((await standings.GetStandingsAsync(leagueId, ct)).Select(e => RegistrationReadDTO.FromEntity(e.Registration, e.RacesCompleted))); }
    [HttpPost("api/v1/leagues/{leagueId}/members")]
    [Authorize]
    public async Task<ActionResult<RegistrationReadDTO>> Register(int leagueId, [FromBody] RegistrationCreateDTO dto, CancellationToken ct)
    { var callerId = auth.GetUserId(User) ?? throw new UnauthorizedAccessException("Authentication is required."); var targetId = dto.UserId ?? callerId; if (targetId != callerId) { var league = await leagues.GetByIdAsync(leagueId, ct); if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource."); } var registration = await service.RegisterAsync(targetId, leagueId, dto.CarNumber, dto.TeamName, dto.BallastKg, ct); return CreatedAtAction(nameof(GetStandings), new { leagueId }, RegistrationReadDTO.FromEntity(registration)); }
    [HttpDelete("api/v1/registrations/{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    { var registration = await service.GetByIdAsync(id, ct); var league = await leagues.GetByIdAsync(registration.LeagueId, ct); var self = auth.GetUserId(User) == registration.UserId; var owner = auth.HasAccessToResource(league.OwnerUserId, User); if (!self && !owner) throw new ForbiddenException("You do not have access to this resource."); await service.DeleteAsync(id, ct); return NoContent(); }
}
