using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

[ApiController, Route("api/v1"), Tags("Race results")]
public sealed class ResultsController(IResultService results, IRaceService races, ILeagueService leagues, IRegistrationService registrations, CsvClassificationParser csvParser, IAuthService auth, LeagueViewPolicy viewPolicy) : ControllerBase
{
    [HttpGet("races/{raceId:int}/results")]
    public async Task<ActionResult<PagedResponse<ResultReadDTO>>> GetByRace(int raceId, [FromQuery] PaginationParameters query, CancellationToken ct)
    {
        var race = await races.GetByIdAsync(raceId, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct);
        if (!await viewPolicy.CanViewAsync(league, User, ct)) throw new ForbiddenException("You do not have access to this resource.");
        var page = await results.GetByRacePageAsync(raceId, query, ct);
        return Ok(new PagedResponse<ResultReadDTO>(page.Items.Select(ResultReadDTO.FromEntity).ToArray(), page.Page, page.PageSize, page.TotalCount));
    }

    [HttpPut("races/{raceId:int}/results"), Authorize]
    public async Task<ActionResult<PagedResponse<ResultReadDTO>>> Replace(int raceId, [FromBody] List<ClassificationEntryDTO> entries, CancellationToken ct)
    {
        var race = await races.GetByIdAsync(raceId, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        await results.ReplaceClassificationAsync(raceId, entries, ct);
        var page = await results.GetByRacePageAsync(raceId, new PaginationParameters { Page = 1, PageSize = 100 }, ct);
        return Ok(new PagedResponse<ResultReadDTO>(page.Items.Select(ResultReadDTO.FromEntity).ToArray(), page.Page, page.PageSize, page.TotalCount));
    }

    /// <summary>Imports a CSV classification, optionally previewing the scored results without saving.</summary>
    /// <remarks>The CSV header must contain position, car number, fastest lap, incidents, DNF, notes.</remarks>
    [HttpPost("races/{raceId:int}/results/import"), Authorize, Consumes("multipart/form-data")]
    [RequestSizeLimit(1_048_576)]
    [ProducesResponseType(typeof(IReadOnlyList<ResultReadDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PagedResponse<ResultReadDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Import(int raceId, [FromForm] IFormFile file, [FromQuery] bool preview, CancellationToken ct)
    {
        if (file is null) throw new DomainValidationException("A CSV file is required.");
        if (file.Length is <= 0 or > 1_048_576) throw new DomainValidationException("CSV file size must be between 1 byte and 1 MB.");
        var race = await races.GetByIdAsync(raceId, ct);
        var league = await leagues.GetByIdAsync(race.LeagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        var members = await registrations.GetByLeagueAsync(league.LeagueId, ct);
        var map = members.ToDictionary(x => x.CarNumber, x => x.RegistrationId);
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        var (entries, errors) = csvParser.Parse(reader, map);
        IReadOnlyList<Result> calculated = [];
        try
        {
            calculated = await results.PreviewClassificationAsync(raceId, entries, ct);
        }
        catch (DomainValidationException exception)
        {
            errors = errors.Append(exception.Message).ToArray();
        }
        if (errors.Count > 0) throw new DomainValidationException(string.Join("; ", errors));

        if (preview)
        {
            return Ok(calculated.Select(ResultReadDTO.FromEntity).ToArray());
        }

        await results.ReplaceClassificationAsync(raceId, entries, ct);
        var page = await results.GetByRacePageAsync(raceId, new PaginationParameters { Page = 1, PageSize = 100 }, ct);
        return StatusCode(StatusCodes.Status201Created,
            new PagedResponse<ResultReadDTO>(page.Items.Select(ResultReadDTO.FromEntity).ToArray(), page.Page, page.PageSize, page.TotalCount));
    }

    [HttpDelete("races/{raceId:int}/results"), Authorize]
    public async Task<IActionResult> Clear(int raceId, CancellationToken ct)
    {
        var race = await races.GetByIdAsync(raceId, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        await results.ClearClassificationAsync(raceId, ct); return NoContent();
    }
}
