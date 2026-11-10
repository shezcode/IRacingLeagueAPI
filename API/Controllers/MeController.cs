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
public sealed class MeController(IUserService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MeProfileDTO>> Get(CancellationToken ct)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(MeProfileDTO.FromEntity(await service.GetByIdAsync(id, ct)));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UserUpdateDTO dto, CancellationToken ct)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await service.UpdateProfileAsync(id, dto.UserName, dto.Tag, ct);
        return NoContent();
    }
}
