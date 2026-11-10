using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

/// <summary>Browses and manages leagues.</summary>
[ApiController]
[Route("api/v1/leagues")]
[Tags("Leagues")]
public sealed class LeaguesController(ILeagueService service, IAuthService auth, LeagueViewPolicy viewPolicy) : ControllerBase
{
    /// <summary>Lists public leagues using filters and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<LeagueReadDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<LeagueReadDTO>>> GetAll([FromQuery] LeagueQueryParameters query, CancellationToken ct)
    {
        var page = await service.GetPublicAsync(query, ct);
        return Ok(new PagedResponse<LeagueReadDTO>(page.Items.Select(LeagueReadDTO.FromEntity).ToArray(), page.Page, page.PageSize, page.TotalCount));
    }

    /// <summary>Gets a league visible to the caller.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LeagueReadDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LeagueReadDTO>> Get(int id, CancellationToken ct)
    {
        var league = await service.GetByIdAsync(id, ct);
        if (!await viewPolicy.CanViewAsync(league, User, ct)) throw new ForbiddenException("You do not have access to this resource.");
        return Ok(LeagueReadDTO.FromEntity(league));
    }

    /// <summary>Creates a league owned by the caller.</summary>
    /// <remarks>Example request: <code>{"name":"Sunday Cup","discipline":"Road","isPublic":true,"maxDrivers":24,"entryFee":0}</code></remarks>
    [HttpPost]
    [Authorize(Roles = $"{nameof(Roles.Driver)},{nameof(Roles.Admin)}")]
    [ProducesResponseType(typeof(LeagueReadDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LeagueReadDTO>> Create([FromBody] LeagueCreateDTO dto, CancellationToken ct)
    {
        var ownerId = auth.GetUserId(User) ?? throw new UnauthorizedAccessException("Authentication is required.");
        var league = await service.CreateAsync(dto.Name, dto.Discipline, dto.IsPublic, dto.MaxDrivers, dto.EntryFee, ownerId, ct);
        return CreatedAtAction(nameof(Get), new { id = league.LeagueId }, LeagueReadDTO.FromEntity(league));
    }

    /// <summary>Updates a league owned by the caller or an administrator.</summary>
    /// <remarks>Example request: <code>{"name":"Sunday Cup 2","discipline":"Road","isPublic":true,"maxDrivers":30,"entryFee":10}</code></remarks>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] LeagueUpdateDTO dto, CancellationToken ct)
    {
        var league = await service.GetByIdAsync(id, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        league.Name = dto.Name; league.Discipline = dto.Discipline; league.IsPublic = dto.IsPublic; league.MaxDrivers = dto.MaxDrivers; league.EntryFee = dto.EntryFee;
        await service.UpdateAsync(league, ct);
        return NoContent();
    }

    /// <summary>Deletes a league and its dependent data.</summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var league = await service.GetByIdAsync(id, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
