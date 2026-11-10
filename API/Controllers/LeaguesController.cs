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
        Response.Headers.ETag = ETag(league.Version);
        return Ok(LeagueReadDTO.FromEntity(league));
    }

    /// <summary>Creates a league owned by the caller.</summary>
    /// <remarks>Example request: <code>{"name":"Sunday Cup","discipline":"Road","isPublic":true,"maxDrivers":24,"entryFee":0,"pointsTable":[25,18,15,12,10,8,6,4,2,1],"fastestLapBonus":1}</code></remarks>
    [HttpPost]
    [Authorize(Roles = $"{nameof(Roles.Driver)},{nameof(Roles.Admin)}")]
    [ProducesResponseType(typeof(LeagueReadDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LeagueReadDTO>> Create([FromBody] LeagueCreateDTO dto, CancellationToken ct)
    {
        var ownerId = auth.GetUserId(User) ?? throw new UnauthorizedAccessException("Authentication is required.");
        var league = await service.CreateAsync(dto.Name, dto.Discipline, dto.IsPublic, dto.MaxDrivers, dto.EntryFee, ownerId, ct, dto.PointsTable, dto.FastestLapBonus);
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] LeagueUpdateDTO dto, [FromHeader(Name = "If-Match")] string? ifMatch, CancellationToken ct)
    {
        var league = await service.GetByIdAsync(id, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        if (!Matches(ifMatch, league.Version)) throw new PreconditionFailedException("The If-Match value does not match the current resource version.");
        league.Name = dto.Name; league.Discipline = dto.Discipline; league.IsPublic = dto.IsPublic; league.MaxDrivers = dto.MaxDrivers; league.EntryFee = dto.EntryFee;
        await service.UpdateAsync(league, dto.PointsTable, dto.FastestLapBonus, ct);
        return NoContent();
    }

    private static string ETag(uint version) => $"\"{version}\"";
    private static bool Matches(string? ifMatch, uint version) => ifMatch is null || ifMatch.Trim() == ETag(version);

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
