using IRacingLeague.Business;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace IRacingLeague.API.Controllers;

[ApiController]
[Route("users")]
public sealed class UsersController(IUserService service, IAuthService auth) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTOOut>>> GetAll([FromQuery] UserQueryParameters query, CancellationToken ct) => Ok((await service.QueryAsync(query, ct)).Select(UserDTOOut.FromEntity));
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDTOOut>> Get(int id, CancellationToken ct) => Ok(UserDTOOut.FromEntity(await service.GetByIdAsync(id, ct)));
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDTO dto, CancellationToken ct)
    { if (!auth.HasAccessToResource(id, User)) throw new ForbiddenException("You do not have access to this resource."); var user = await service.GetByIdAsync(id, ct); user.UserName = dto.UserName; user.Tag = dto.Tag; user.LicenseClass = dto.LicenseClass; await service.UpdateAsync(user, ct); return NoContent(); }
    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) { await service.DeleteAsync(id, ct); return NoContent(); }
}
