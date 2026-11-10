using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

[ApiController, Route("api/v1"), Tags("Race results")]
public sealed class ResultsController(IResultService results, IRaceService races, ILeagueService leagues, IAuthService auth, LeagueViewPolicy viewPolicy) : ControllerBase
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

    [HttpDelete("races/{raceId:int}/results"), Authorize]
    public async Task<IActionResult> Clear(int raceId, CancellationToken ct)
    {
        var race = await races.GetByIdAsync(raceId, ct); var league = await leagues.GetByIdAsync(race.LeagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        await results.ClearClassificationAsync(raceId, ct); return NoContent();
    }
}
