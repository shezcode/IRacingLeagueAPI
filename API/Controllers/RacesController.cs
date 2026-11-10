using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace IRacingLeague.API.Controllers;

[ApiController]
public sealed class RacesController(IRaceService service, ILeagueService leagues, IAuthService auth, LeagueViewPolicy viewPolicy) : ControllerBase
{
    [HttpGet("api/v1/leagues/{leagueId}/races")]
    public async Task<ActionResult<IEnumerable<RaceReadDTO>>> GetByLeague(int leagueId, [FromQuery] RaceQueryParameters query, CancellationToken ct)
    { var league = await leagues.GetByIdAsync(leagueId, ct); if (!await viewPolicy.CanViewAsync(league, User, ct)) throw new ForbiddenException("You do not have access to this resource."); return Ok((await service.GetByLeagueAsync(leagueId, query, ct)).Select(RaceReadDTO.FromEntity)); }
    [HttpGet("api/v1/races/{id}")]
    public async Task<ActionResult<RaceReadDTO>> Get(int id, CancellationToken ct)
    { var race = await service.GetByIdAsync(id, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct); if (!await viewPolicy.CanViewAsync(league, User, ct)) throw new ForbiddenException("You do not have access to this resource."); return Ok(RaceReadDTO.FromEntity(race)); }
    [HttpPost("api/v1/leagues/{leagueId}/races")]
    [Authorize]
    public async Task<ActionResult<RaceReadDTO>> Create(int leagueId, [FromBody] RaceCreateDTO dto, CancellationToken ct)
    { var league = await leagues.GetByIdAsync(leagueId, ct); if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource."); var race = await service.CreateAsync(leagueId, dto.Track, dto.Car, dto.ScheduledAt, dto.LapCount, dto.AmbientTempC, ct); return CreatedAtAction(nameof(Get), new { id = race.RaceId }, RaceReadDTO.FromEntity(race)); }
    [HttpPut("api/v1/races/{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] RaceUpdateDTO dto, CancellationToken ct)
    { var race = await service.GetByIdAsync(id, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct); if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource."); race.Track = dto.Track; race.Car = dto.Car; race.ScheduledAt = dto.ScheduledAt; race.LapCount = dto.LapCount; race.AmbientTempC = dto.AmbientTempC; race.IsCompleted = dto.IsCompleted; await service.UpdateAsync(race, ct); return NoContent(); }
    [HttpDelete("api/v1/races/{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    { var race = await service.GetByIdAsync(id, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct); if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource."); await service.DeleteAsync(id, ct); return NoContent(); }
}
