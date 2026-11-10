using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

/// <summary>Reads and records race results.</summary>
[ApiController]
[Route("api/v1")]
[Tags("Race results")]
public sealed class ResultsController(IResultService results, IRegistrationService registrations, IRaceService races, ILeagueService leagues, IAuthService auth, LeagueViewPolicy viewPolicy) : ControllerBase
{
    /// <summary>Lists a race's results in position order.</summary>
    [HttpGet("races/{raceId:int}/results")]
    [ProducesResponseType(typeof(PagedResponse<ResultReadDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<ResultReadDTO>>> GetByRace(int raceId, [FromQuery] PaginationParameters query, CancellationToken ct)
    {
        var race = await races.GetByIdAsync(raceId, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct);
        if (!await viewPolicy.CanViewAsync(league, User, ct)) throw new ForbiddenException("You do not have access to this resource.");
        var page = await results.GetByRacePageAsync(raceId, query, ct);
        return Ok(new PagedResponse<ResultReadDTO>(page.Items.Select(ResultReadDTO.FromEntity).ToArray(), page.Page, page.PageSize, page.TotalCount));
    }

    /// <summary>Creates or replaces one driver's result for a race.</summary>
    /// <remarks>Example request: <code>{"registrationId":12,"position":1,"fastestLapSeconds":92.5,"points":25,"incidentPoints":0,"dnf":false,"notes":"Clean race"}</code></remarks>
    [HttpPost("races/{raceId:int}/results")]
    [Authorize]
    [ProducesResponseType(typeof(ResultReadDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResultReadDTO>> Create(int raceId, [FromBody] ResultCreateDTO dto, CancellationToken ct)
    {
        var race = await races.GetByIdAsync(raceId, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        var registration = await registrations.GetByIdAsync(dto.RegistrationId, ct);
        if (registration.LeagueId != race.LeagueId) throw new DomainValidationException("Registration does not belong to this race's league.");
        var result = new Result(registration.RegistrationId, race.RaceId, dto.Position, dto.FastestLapSeconds, dto.Points, dto.IncidentPoints, dto.Dnf, dto.Notes);
        var applied = await results.ApplyResultAsync(registration, race, result, ct);
        return CreatedAtAction(nameof(GetByRace), new { raceId }, ResultReadDTO.FromEntity(applied));
    }

    /// <summary>Updates one driver's result for a race.</summary>
    /// <remarks>Example request: <code>{"registrationId":12,"position":2,"fastestLapSeconds":93.1,"points":18,"incidentPoints":2,"dnf":false,"notes":""}</code></remarks>
    [HttpPut("races/{raceId:int}/results/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int raceId, int id, [FromBody] ResultCreateDTO dto, CancellationToken ct)
    {
        var result = await results.GetByIdAsync(id, ct);
        if (result.RaceId != raceId) throw new IRacingLeague.Business.NotFoundException($"Result {id} was not found in race {raceId}.");
        var race = await races.GetByIdAsync(result.RaceId, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        if (dto.RegistrationId != result.RegistrationId) throw new DomainValidationException("Cannot reassign a result to a different registration.");
        var registration = await registrations.GetByIdAsync(result.RegistrationId, ct);
        var updated = new Result(registration.RegistrationId, race.RaceId, dto.Position, dto.FastestLapSeconds, dto.Points, dto.IncidentPoints, dto.Dnf, dto.Notes);
        await results.ApplyResultAsync(registration, race, updated, ct); return NoContent();
    }

    /// <summary>Deletes one race result.</summary>
    [HttpDelete("races/{raceId:int}/results/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int raceId, int id, CancellationToken ct)
    {
        var result = await results.GetByIdAsync(id, ct);
        if (result.RaceId != raceId) throw new IRacingLeague.Business.NotFoundException($"Result {id} was not found in race {raceId}.");
        var race = await races.GetByIdAsync(result.RaceId, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        await results.DeleteAsync(id, ct); return NoContent();
    }
}
