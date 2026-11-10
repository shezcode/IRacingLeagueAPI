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

            var leagues = _service.GetPublic(query).Select(LeagueReadDTO.FromEntity);
            return Ok(leagues);

    }

    [HttpGet("{id}")]
    public ActionResult<LeagueReadDTO> Get(int id)
    {

            var league = _service.GetById(id);
            if (!_viewPolicy.CanView(league, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            return Ok(LeagueReadDTO.FromEntity(league));

    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(Roles.Driver)},{nameof(Roles.Admin)}")]
    public ActionResult<LeagueReadDTO> Create([FromBody] LeagueCreateDTO dto)
    {
        var ownerUserId = _auth.GetUserId(User);
        if (ownerUserId is null)
            throw new UnauthorizedAccessException("Authentication is required.");


            var league = _service.Create(
                dto.Name, dto.Discipline, dto.IsPublic, dto.MaxDrivers, dto.EntryFee, ownerUserId.Value);

            return CreatedAtAction(nameof(Get), new { id = league.LeagueId }, LeagueReadDTO.FromEntity(league));

    }

    [HttpPut("{id}")]
    [Authorize]
    public IActionResult Update(int id, [FromBody] LeagueUpdateDTO dto)
    {

            var league = _service.GetById(id);   // throws KeyNotFoundException when missing
            if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            league.Name = dto.Name;
            league.Discipline = dto.Discipline;
            league.IsPublic = dto.IsPublic;
            league.MaxDrivers = dto.MaxDrivers;
            league.EntryFee = dto.EntryFee;

            _service.Update(league);
            return NoContent();

    }

    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult Delete(int id)
    {

            var league = _service.GetById(id);   // throws KeyNotFoundException when missing
            if (!_auth.HasAccessToResource(league.OwnerUserId, User))
                throw new IRacingLeague.Business.ForbiddenException("You do not have access to this resource.");

            _service.Delete(id);
            return NoContent();

    }
}
