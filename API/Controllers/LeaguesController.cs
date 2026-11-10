using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace IRacingLeague.API.Controllers;

[ApiController]
[Route("api/v1/leagues")]
public sealed class LeaguesController(ILeagueService service, IAuthService auth, LeagueViewPolicy viewPolicy) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeagueReadDTO>>> GetAll([FromQuery] LeagueQueryParameters query, CancellationToken ct) => Ok((await service.GetPublicAsync(query, ct)).Select(LeagueReadDTO.FromEntity));
    [HttpGet("{id}")]
    public async Task<ActionResult<LeagueReadDTO>> Get(int id, CancellationToken ct)
    { var league = await service.GetByIdAsync(id, ct); if (!await viewPolicy.CanViewAsync(league, User, ct)) throw new ForbiddenException("You do not have access to this resource."); return Ok(LeagueReadDTO.FromEntity(league)); }
    [HttpPost]
    [Authorize(Roles = $"{nameof(Roles.Driver)},{nameof(Roles.Admin)}")]
    public async Task<ActionResult<LeagueReadDTO>> Create([FromBody] LeagueCreateDTO dto, CancellationToken ct)
    { var ownerId = auth.GetUserId(User) ?? throw new UnauthorizedAccessException("Authentication is required."); var league = await service.CreateAsync(dto.Name, dto.Discipline, dto.IsPublic, dto.MaxDrivers, dto.EntryFee, ownerId, ct); return CreatedAtAction(nameof(Get), new { id = league.LeagueId }, LeagueReadDTO.FromEntity(league)); }
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] LeagueUpdateDTO dto, CancellationToken ct)
    { var league = await service.GetByIdAsync(id, ct); if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource."); league.Name = dto.Name; league.Discipline = dto.Discipline; league.IsPublic = dto.IsPublic; league.MaxDrivers = dto.MaxDrivers; league.EntryFee = dto.EntryFee; await service.UpdateAsync(league, ct); return NoContent(); }
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    { var league = await service.GetByIdAsync(id, ct); if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource."); await service.DeleteAsync(id, ct); return NoContent(); }
}
