using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

[ApiController]
public class ResultsController : ControllerBase
{
    private readonly IResultService _results;
    private readonly IRegistrationService _registrations;
    private readonly IRaceService _races;
    private readonly ILeagueService _leagues;
    private readonly IAuthService _auth;
    private readonly LeagueViewPolicy _viewPolicy;
    private readonly ILogger<ResultsController> _logger;

    public ResultsController(
        IResultService results, IRegistrationService registrations, IRaceService races,
        ILeagueService leagues, IAuthService auth, LeagueViewPolicy viewPolicy,
        ILogger<ResultsController> logger)
    {
        _results = results;
        _registrations = registrations;
        _races = races;
        _leagues = leagues;
        _auth = auth;
        _viewPolicy = viewPolicy;
        _logger = logger;
    }

    [HttpGet("races/{raceId}/results")]
    public ActionResult<IEnumerable<ResultReadDTO>> GetByRace(int raceId)
    {

            var race = _races.GetById(raceId);   // 404 if missing
            var league = _leagues.GetById(race.LeagueId);
            if (!_viewPolicy.CanView(league, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            var results = _results.GetByRace(raceId).Select(ResultReadDTO.FromEntity);
            return Ok(results);

    }

    [HttpPost("races/{raceId}/results")]
    [Authorize]
    public ActionResult<ResultReadDTO> Create(int raceId, [FromBody] ResultCreateDTO dto)
    {

            var race = _races.GetById(raceId);              // 404 if the race is missing
            var league = _leagues.GetById(race.LeagueId);
            if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            var registration = _registrations.GetById(dto.RegistrationId);  // 404 if missing

            // The registration must belong to the same league as the race; otherwise a
            // driver from another league could be scored here. Abort without persisting.
            if (registration.LeagueId != race.LeagueId)
                throw new IRacingLeague.Business.DomainValidationException("Registration does not belong to this race's league.");

            var result = new Result(
                registration.RegistrationId, race.RaceId, dto.Position,
                dto.FastestLapSeconds, dto.Points, dto.IncidentPoints, dto.Dnf, dto.Notes);

            var applied = _results.ApplyResult(registration, race, result);
            return CreatedAtAction(nameof(GetByRace), new { raceId }, ResultReadDTO.FromEntity(applied));

    }

    [HttpPut("results/{id}")]
    [Authorize]
    public IActionResult Update(int id, [FromBody] ResultCreateDTO dto)
    {

            var result = _results.GetById(id);          // 404 if missing
            var race = _races.GetById(result.RaceId);
            var league = _leagues.GetById(race.LeagueId);
            if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            // A result edit stays with its original driver. Reassigning to another
            // registration would touch two drivers' stats — that's a delete + re-create,
            // not an edit — so it's rejected here.
            if (dto.RegistrationId != result.RegistrationId)
                throw new IRacingLeague.Business.DomainValidationException("Cannot reassign a result to a different registration.");

            var registration = _registrations.GetById(result.RegistrationId);

            // Route through ApplyResult so the previous points/iRating/SR/wins contribution
            // is backed out and the new values applied — keeping standings and stats in sync.
            var updated = new Result(
                registration.RegistrationId, race.RaceId, dto.Position,
                dto.FastestLapSeconds, dto.Points, dto.IncidentPoints, dto.Dnf, dto.Notes);

            _results.ApplyResult(registration, race, updated);
            return NoContent();

    }

    [HttpDelete("results/{id}")]
    [Authorize]
    public IActionResult Delete(int id)
    {

            var result = _results.GetById(id);          // 404 if missing
            var race = _races.GetById(result.RaceId);
            var league = _leagues.GetById(race.LeagueId);
            if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            // Backs out the result's points/iRating/SR/wins contribution before removing it,
            // keeping standings and global driver stats in sync.
            _results.Delete(id);
            return NoContent();

    }
}
