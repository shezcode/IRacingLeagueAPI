using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

/// <summary>Schedules and reads league races.</summary>
[ApiController]
[Route("api/v1")]
[Tags("Races")]
public sealed class RacesController(IRaceService service, ILeagueService leagues, IAuthService auth, LeagueViewPolicy viewPolicy) : ControllerBase
{
    /// <summary>Lists a league's races using filters and pagination.</summary>
    [HttpGet("leagues/{leagueId:int}/races")]
    [ProducesResponseType(typeof(PagedResponse<RaceReadDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<RaceReadDTO>>> GetByLeague(int leagueId, [FromQuery] RaceQueryParameters query, CancellationToken ct)
    {
        var league = await leagues.GetByIdAsync(leagueId, ct);
        if (!await viewPolicy.CanViewAsync(league, User, ct)) throw new ForbiddenException("You do not have access to this resource.");
        var page = await service.GetByLeagueAsync(leagueId, query, ct);
        return Ok(new PagedResponse<RaceReadDTO>(page.Items.Select(RaceReadDTO.FromEntity).ToArray(), page.Page, page.PageSize, page.TotalCount));
    }

    /// <summary>Gets a race visible to the caller.</summary>
    [HttpGet("races/{id:int}")]
    [ProducesResponseType(typeof(RaceReadDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RaceReadDTO>> Get(int id, CancellationToken ct)
    {
        var race = await service.GetByIdAsync(id, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct);
        if (!await viewPolicy.CanViewAsync(league, User, ct)) throw new ForbiddenException("You do not have access to this resource.");
        Response.Headers.ETag = ETag(race.Version);
        return Ok(RaceReadDTO.FromEntity(race));
    }

    /// <summary>Schedules a race in a league.</summary>
    /// <remarks>Example request: <code>{"track":"Spa-Francorchamps","car":"GT3","scheduledAt":"2027-03-14T14:00:00Z","lapCount":20,"ambientTempC":18}</code></remarks>
    [HttpPost("leagues/{leagueId:int}/races")]
    [Authorize]
    [ProducesResponseType(typeof(RaceReadDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RaceReadDTO>> Create(int leagueId, [FromBody] RaceCreateDTO dto, CancellationToken ct)
    {
        var league = await leagues.GetByIdAsync(leagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        var race = await service.CreateAsync(leagueId, dto.Track, dto.Car, dto.ScheduledAt, dto.LapCount, dto.AmbientTempC, ct);
        return CreatedAtAction(nameof(Get), new { id = race.RaceId }, RaceReadDTO.FromEntity(race));
    }

    /// <summary>Updates a scheduled race.</summary>
    /// <remarks>Example request: <code>{"track":"Monza","car":"GT3","scheduledAt":"2027-04-04T14:00:00Z","lapCount":18,"ambientTempC":20,"isCompleted":false}</code></remarks>
    [HttpPut("races/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] RaceUpdateDTO dto, [FromHeader(Name = "If-Match")] string? ifMatch, CancellationToken ct)
    {
        var race = await service.GetByIdAsync(id, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        if (!Matches(ifMatch, race.Version)) throw new PreconditionFailedException("The If-Match value does not match the current resource version.");
        race.Track = dto.Track; race.Car = dto.Car; race.ScheduledAt = dto.ScheduledAt; race.LapCount = dto.LapCount; race.AmbientTempC = dto.AmbientTempC; race.IsCompleted = dto.IsCompleted;
        await service.UpdateAsync(race, ct); return NoContent();
    }

    private static string ETag(uint version) => $"\"{version}\"";
    private static bool Matches(string? ifMatch, uint version) => ifMatch is null || ifMatch.Trim() == ETag(version);

    /// <summary>Deletes a race and its results.</summary>
    [HttpDelete("races/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var race = await service.GetByIdAsync(id, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        await service.DeleteAsync(id, ct); return NoContent();
    }
}
