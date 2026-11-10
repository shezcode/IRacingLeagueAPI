using IRacingLeague.Business;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            var users = _service.Query(query).Select(UserDTOOut.FromEntity);
            return Ok(users);

    }

    [HttpGet("{id}")]
    public ActionResult<UserDTOOut> Get(int id)
    {

            var user = _service.GetById(id);
            return Ok(UserDTOOut.FromEntity(user));

    }

    [HttpPut("{id}")]
    [Authorize]
    public IActionResult Update(int id, [FromBody] UserUpdateDTO dto)
    {
        if (!_auth.HasAccessToResource(id, User))   // self (id == caller) or Admin
            throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");


            var user = _service.GetById(id);   // throws KeyNotFoundException when missing
            user.UserName = dto.UserName;
            user.Tag = dto.Tag;
            user.LicenseClass = dto.LicenseClass;

            _service.Update(user);
            return NoContent();

    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Roles.Admin))]
    public IActionResult Delete(int id)
    {

            // Cascades the user's own registrations (and their results); refuses if the user
            // still owns any league (UserOwnsLeaguesException -> 409).
            _service.Delete(id);
            return NoContent();

    }
}
