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

            var league = _leagues.GetById(leagueId);   // 404 if missing
            if (!_viewPolicy.CanView(league, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            var races = _service.GetByLeague(leagueId, query).Select(RaceReadDTO.FromEntity);
            return Ok(races);

    }

    [HttpGet("races/{id}")]
    public ActionResult<RaceReadDTO> Get(int id)
    {

            var race = _service.GetById(id);
            var league = _leagues.GetById(race.LeagueId);
            if (!_viewPolicy.CanView(league, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            return Ok(RaceReadDTO.FromEntity(race));

    }

    [HttpPost("leagues/{leagueId}/races")]
    [Authorize]
    public ActionResult<RaceReadDTO> Create(int leagueId, [FromBody] RaceCreateDTO dto)
    {

            var league = _leagues.GetById(leagueId);   // 404 if missing
            if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            var race = _service.Create(
                leagueId, dto.Track, dto.Car, dto.ScheduledAt, dto.LapCount, dto.AmbientTempC);

            return CreatedAtAction(nameof(Get), new { id = race.RaceId }, RaceReadDTO.FromEntity(race));

    }

    [HttpPut("races/{id}")]
    [Authorize]
    public IActionResult Update(int id, [FromBody] RaceUpdateDTO dto)
    {

            var race = _service.GetById(id);   // 404 if missing
            var league = _leagues.GetById(race.LeagueId);
            if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            race.Track = dto.Track;
            race.Car = dto.Car;
            race.ScheduledAt = dto.ScheduledAt;
            race.LapCount = dto.LapCount;
            race.AmbientTempC = dto.AmbientTempC;
            // Round is derived from schedule order, not client-supplied; left untouched here.
            race.IsCompleted = dto.IsCompleted;

            _service.Update(race);
            return NoContent();

    }

    [HttpDelete("races/{id}")]
    [Authorize]
    public IActionResult Delete(int id)
    {

            var race = _service.GetById(id);   // 404 if missing
            var league = _leagues.GetById(race.LeagueId);
            if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            _service.Delete(id);
            return NoContent();

    }
}
