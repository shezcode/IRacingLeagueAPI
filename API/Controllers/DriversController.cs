using IRacingLeague.Business;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

[ApiController]
[Route("api/v1/drivers")]
[Tags("Drivers")]
public sealed class DriversController(IUserService service, IAuthService auth) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<DriverProfileDTO>>> GetAll([FromQuery] UserQueryParameters query, CancellationToken ct)
    {
        var page = await service.QueryAsync(query, ct);
        return Ok(new PagedResponse<DriverProfileDTO>(page.Items.Select(DriverProfileDTO.FromEntity).ToArray(), page.Page, page.PageSize, page.TotalCount));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DriverProfileDTO>> Get(int id, CancellationToken ct) => Ok(DriverProfileDTO.FromEntity(await service.GetByIdAsync(id, ct)));

    /// <summary>Gets a driver's career totals and rating history from leagues visible to the caller.</summary>
    [HttpGet("{id:int}/stats")]
    [ProducesResponseType(typeof(DriverStatsDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DriverStatsDTO>> GetStats(int id, CancellationToken ct) =>
        Ok(await service.GetDriverStatsAsync(id, auth.GetUserId(User), User.IsInRole(nameof(Roles.Admin)), ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) { await service.DeleteAsync(id, ct); return NoContent(); }
}
