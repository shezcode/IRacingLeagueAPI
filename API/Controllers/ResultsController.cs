using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace IRacingLeague.API.Controllers;

[ApiController]
public sealed class ResultsController(IResultService results, IRegistrationService registrations, IRaceService races, ILeagueService leagues, IAuthService auth, LeagueViewPolicy viewPolicy) : ControllerBase
{
    [HttpGet("races/{raceId}/results")]
    public async Task<ActionResult<IEnumerable<ResultReadDTO>>> GetByRace(int raceId, CancellationToken ct)
    { var race = await races.GetByIdAsync(raceId, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct); if (!await viewPolicy.CanViewAsync(league, User, ct)) throw new ForbiddenException("You do not have access to this resource."); return Ok((await results.GetByRaceAsync(raceId, ct)).Select(ResultReadDTO.FromEntity)); }
    [HttpPost("races/{raceId}/results")]
    [Authorize]
    public async Task<ActionResult<ResultReadDTO>> Create(int raceId, [FromBody] ResultCreateDTO dto, CancellationToken ct)
    {
        var race = await races.GetByIdAsync(raceId, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct); if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        var registration = await registrations.GetByIdAsync(dto.RegistrationId, ct); if (registration.LeagueId != race.LeagueId) throw new DomainValidationException("Registration does not belong to this race's league.");
        var result = new Result(registration.RegistrationId, race.RaceId, dto.Position, dto.FastestLapSeconds, dto.Points, dto.IncidentPoints, dto.Dnf, dto.Notes); var applied = await results.ApplyResultAsync(registration, race, result, ct); return CreatedAtAction(nameof(GetByRace), new { raceId }, ResultReadDTO.FromEntity(applied));
    }
    [HttpPut("results/{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] ResultCreateDTO dto, CancellationToken ct)
    {
        var result = await results.GetByIdAsync(id, ct); var race = await races.GetByIdAsync(result.RaceId, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct); if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        if (dto.RegistrationId != result.RegistrationId) throw new DomainValidationException("Cannot reassign a result to a different registration."); var registration = await registrations.GetByIdAsync(result.RegistrationId, ct); var updated = new Result(registration.RegistrationId, race.RaceId, dto.Position, dto.FastestLapSeconds, dto.Points, dto.IncidentPoints, dto.Dnf, dto.Notes); await results.ApplyResultAsync(registration, race, updated, ct); return NoContent();
    }
    [HttpDelete("results/{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    { var result = await results.GetByIdAsync(id, ct); var race = await races.GetByIdAsync(result.RaceId, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct); if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource."); await results.DeleteAsync(id, ct); return NoContent(); }
}
