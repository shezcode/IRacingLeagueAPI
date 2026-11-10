using System.Security.Claims;
using IRacingLeague.Business;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

[ApiController]
[Route("api/v1/me")]
[Authorize]
[Tags("Profile")]
public sealed class MeController(IUserService service, ILeagueService leagues) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MeProfileDTO>> Get(CancellationToken ct)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(MeProfileDTO.FromEntity(await service.GetByIdAsync(id, ct)));
    }

    [HttpGet("leagues")]
    [ProducesResponseType(typeof(PagedResponse<LeagueReadDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<LeagueReadDTO>>> GetLeagues([FromQuery] PaginationParameters query, CancellationToken ct)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var page = await leagues.GetForUserAsync(id, query, ct);
        return Ok(new PagedResponse<LeagueReadDTO>(page.Items.Select(LeagueReadDTO.FromEntity).ToArray(), page.Page, page.PageSize, page.TotalCount));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UserUpdateDTO dto, CancellationToken ct)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await service.UpdateProfileAsync(id, dto.UserName, dto.Tag, ct);
        return NoContent();
    }
}
