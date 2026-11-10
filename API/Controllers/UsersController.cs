using IRacingLeague.Business;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

/// <summary>Browses and manages driver accounts.</summary>
[ApiController]
[Route("api/v1/users")]
[Tags("Users")]
public sealed class UsersController(IUserService service, IAuthService auth) : ControllerBase
{
    /// <summary>Lists users with rating and license filters and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserDTOOut>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<UserDTOOut>>> GetAll([FromQuery] UserQueryParameters query, CancellationToken ct)
    {
        var page = await service.QueryAsync(query, ct);
        return Ok(new PagedResponse<UserDTOOut>(page.Items.Select(UserDTOOut.FromEntity).ToArray(), page.Page, page.PageSize, page.TotalCount));
    }

    /// <summary>Gets a user profile.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserDTOOut), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDTOOut>> Get(int id, CancellationToken ct) => Ok(UserDTOOut.FromEntity(await service.GetByIdAsync(id, ct)));

    /// <summary>Updates a user's editable profile fields.</summary>
    /// <remarks>Example request: <code>{"userName":"Ayrton Senna","tag":"SEN","licenseClass":"Rookie"}</code></remarks>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDTO dto, CancellationToken ct)
    {
        if (!auth.HasAccessToResource(id, User)) throw new ForbiddenException("You do not have access to this resource.");
        var user = await service.GetByIdAsync(id, ct); user.UserName = dto.UserName; user.Tag = dto.Tag; user.LicenseClass = dto.LicenseClass;
        await service.UpdateAsync(user, ct); return NoContent();
    }

    /// <summary>Deletes a user account.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) { await service.DeleteAsync(id, ct); return NoContent(); }
}
