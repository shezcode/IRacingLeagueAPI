using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IRacingLeague.API.Controllers;

[ApiController]
public class RacesController : ControllerBase
{
    private readonly IRaceService _service;
    private readonly ILeagueService _leagues;
    private readonly IAuthService _auth;
    private readonly LeagueViewPolicy _viewPolicy;
    private readonly ILogger<RacesController> _logger;

    public RacesController(
        IRaceService service, ILeagueService leagues, IAuthService auth,
        LeagueViewPolicy viewPolicy, ILogger<RacesController> logger)
    {
        _service = service;
        _leagues = leagues;
        _auth = auth;
        _viewPolicy = viewPolicy;
        _logger = logger;
    }

    [HttpGet("leagues/{leagueId}/races")]
    public ActionResult<IEnumerable<RaceReadDTO>> GetByLeague(int leagueId, [FromQuery] RaceQueryParameters query)
    {
        try
        {
            var league = _leagues.GetById(leagueId);   // 404 if missing
            if (!_viewPolicy.CanView(league, User))
                return Forbid();

            var races = _service.GetByLeague(leagueId, query).Select(RaceReadDTO.FromEntity);
            return Ok(races);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list races for league {LeagueId}", leagueId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("races/{id}")]
    public ActionResult<RaceReadDTO> Get(int id)
    {
        try
        {
            var race = _service.GetById(id);
            var league = _leagues.GetById(race.LeagueId);
            if (!_viewPolicy.CanView(league, User))
                return Forbid();

            return Ok(RaceReadDTO.FromEntity(race));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch race {RaceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("leagues/{leagueId}/races")]
    [Authorize]
    public ActionResult<RaceReadDTO> Create(int leagueId, [FromBody] RaceCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var league = _leagues.GetById(leagueId);   // 404 if missing
            if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                return Forbid();

            var race = _service.Create(
                leagueId, dto.Track, dto.Car, dto.ScheduledAt, dto.LapCount, dto.AmbientTempC, dto.Round);

            return CreatedAtAction(nameof(Get), new { id = race.RaceId }, RaceReadDTO.FromEntity(race));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule race in league {LeagueId}", leagueId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("races/{id}")]
    [Authorize]
    public IActionResult Update(int id, [FromBody] RaceUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var race = _service.GetById(id);   // 404 if missing
            var league = _leagues.GetById(race.LeagueId);
            if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                return Forbid();

            race.Track = dto.Track;
            race.Car = dto.Car;
            race.ScheduledAt = dto.ScheduledAt;
            race.LapCount = dto.LapCount;
            race.AmbientTempC = dto.AmbientTempC;
            race.Round = dto.Round;
            race.IsCompleted = dto.IsCompleted;

            _service.Update(race);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update race {RaceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("races/{id}")]
    [Authorize]
    public IActionResult Delete(int id)
    {
        try
        {
            var race = _service.GetById(id);   // 404 if missing
            var league = _leagues.GetById(race.LeagueId);
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
            // FK is Restrict (AppDbContext): a race with recorded results can't be removed until those results are cleared. 
            return Conflict("Cannot delete a race that has recorded results.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete race {RaceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
