using IRacingLeague.Business;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    private readonly IAuthService _auth;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService service, IAuthService auth, ILogger<UsersController> logger)
    {
        _service = service;
        _auth = auth;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserDTOOut>> GetAll([FromQuery] UserQueryParameters query)
    {
        try
        {
            var users = _service.Query(query).Select(UserDTOOut.FromEntity);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list users");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id}")]
    public ActionResult<UserDTOOut> Get(int id)
    {
        try
        {
            var user = _service.GetById(id);
            return Ok(UserDTOOut.FromEntity(user));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public IActionResult Update(int id, [FromBody] UserUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_auth.HasAccessToResource(id, User))   // self (id == caller) or Admin
            return Forbid();

        try
        {
            var user = _service.GetById(id);   // throws KeyNotFoundException when missing
            user.UserName = dto.UserName;
            user.Tag = dto.Tag;
            user.LicenseClass = dto.LicenseClass;

            _service.Update(user);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
