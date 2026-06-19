using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.API.Controllers;

[ApiController]
[Route("[controller]")]
public class LeaguesController : ControllerBase
{
    private readonly ILeagueService _service;
    private readonly IAuthService _auth;
    private readonly LeagueViewPolicy _viewPolicy;
    private readonly ILogger<LeaguesController> _logger;

    public LeaguesController(
        ILeagueService service, IAuthService auth,
        LeagueViewPolicy viewPolicy, ILogger<LeaguesController> logger)
    {
        _service = service;
        _auth = auth;
        _viewPolicy = viewPolicy;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<LeagueReadDTO>> GetAll([FromQuery] LeagueQueryParameters query)
    {
        try
        {
            var leagues = _service.GetPublic(query).Select(LeagueReadDTO.FromEntity);
            return Ok(leagues);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list public leagues");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id}")]
    public ActionResult<LeagueReadDTO> Get(int id)
    {
        try
        {
            var league = _service.GetById(id);
            if (!_viewPolicy.CanView(league, User))
                return Forbid();

            return Ok(LeagueReadDTO.FromEntity(league));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch league {LeagueId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(Roles.Driver)},{nameof(Roles.Admin)}")]
    public ActionResult<LeagueReadDTO> Create([FromBody] LeagueCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ownerUserId = _auth.GetUserId(User);
        if (ownerUserId is null)
            return Unauthorized();

        try
        {
            var league = _service.Create(
                dto.Name, dto.Discipline, dto.IsPublic, dto.MaxDrivers, dto.EntryFee, ownerUserId.Value);

            return CreatedAtAction(nameof(Get), new { id = league.LeagueId }, LeagueReadDTO.FromEntity(league));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create league");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public IActionResult Update(int id, [FromBody] LeagueUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var league = _service.GetById(id);   // throws KeyNotFoundException when missing
            if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                return Forbid();

            league.Name = dto.Name;
            league.Discipline = dto.Discipline;
            league.IsPublic = dto.IsPublic;
            league.MaxDrivers = dto.MaxDrivers;
            league.EntryFee = dto.EntryFee;

            _service.Update(league);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update league {LeagueId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult Delete(int id)
    {
        try
        {
            var league = _service.GetById(id);   // throws KeyNotFoundException when missing
            if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                return Forbid();

            _service.Delete(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (DbUpdateException)
        {
            // Defensive fallback: the service cascades registrations, races, and their results
            // before removing the league, so the Restrict FK shouldn't trip — but surface a 409
            // rather than a 500 if it does.
            return Conflict("Cannot delete a league that still has registrations or races.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete league {LeagueId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
