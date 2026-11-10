using IRacingLeague.API.Authorization;
using IRacingLeague.Business;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

/// <summary>Manages league members and reads championship standings.</summary>
[ApiController]
[Route("api/v1/leagues/{leagueId:int}")]
[Tags("League members", "Standings")]
public sealed class RegistrationsController(IRegistrationService service, IStandingsService standings, ILeagueService leagues, IUserService users, IAuthService auth, LeagueViewPolicy viewPolicy) : ControllerBase
{
    /// <summary>Lists members in the league.</summary>
    [HttpGet("members")]
    [ProducesResponseType(typeof(PagedResponse<RegistrationReadDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<RegistrationReadDTO>>> GetMembers(int leagueId, [FromQuery] PaginationParameters query, CancellationToken ct)
    {
        var league = await leagues.GetByIdAsync(leagueId, ct);
        if (!await viewPolicy.CanViewAsync(league, User, ct)) throw new ForbiddenException("You do not have access to this resource.");
        var page = await service.GetByLeaguePageAsync(leagueId, query, ct);
        return Ok(new PagedResponse<RegistrationReadDTO>(page.Items.Select(x => RegistrationReadDTO.FromEntity(x)).ToArray(), page.Page, page.PageSize, page.TotalCount));
    }

    /// <summary>Lists the league standings.</summary>
    [HttpGet("standings")]
    [ProducesResponseType(typeof(PagedResponse<RegistrationReadDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<RegistrationReadDTO>>> GetStandings(int leagueId, [FromQuery] PaginationParameters query, CancellationToken ct)
    {
        var league = await leagues.GetByIdAsync(leagueId, ct);
        if (!await viewPolicy.CanViewAsync(league, User, ct)) throw new ForbiddenException("You do not have access to this resource.");
        var page = await standings.GetStandingsAsync(leagueId, query, ct);
        return Ok(new PagedResponse<RegistrationReadDTO>(page.Items.Select((e, i) => RegistrationReadDTO.FromStanding(new IRacingLeague.Models.StandingSummary(e.Registration, e.RacesCompleted, e.Points, e.Wins, e.Podiums, e.IncidentPoints, e.BestFinish), (query.Page - 1) * query.PageSize + i + 1)).ToArray(), page.Page, page.PageSize, page.TotalCount));
    }

    /// <summary>Joins the caller or adds a driver to the league.</summary>
    /// <remarks>Example request: <code>{"carNumber":44,"teamName":"Team Lotus","ballastKg":0}</code></remarks>
    [HttpPost("members")]
    [Authorize]
    [ProducesResponseType(typeof(RegistrationReadDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RegistrationReadDTO>> Register(int leagueId, [FromBody] RegistrationCreateDTO dto, CancellationToken ct)
    {
        var callerId = auth.GetUserId(User) ?? throw new UnauthorizedAccessException("Authentication is required."); var targetId = dto.UserId ?? callerId;
        var league = await leagues.GetByIdAsync(leagueId, ct);
        if (targetId != callerId)
        {
            if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
            await users.GetByIdAsync(targetId, ct);
        }
        var registration = await service.RegisterAsync(targetId, leagueId, dto.CarNumber, dto.TeamName, dto.BallastKg, dto.InviteToken, targetId == callerId, ct);
        return CreatedAtAction(nameof(GetMembers), new { leagueId }, RegistrationReadDTO.FromEntity(registration));
    }

    /// <summary>Creates a single-use invite for the league.</summary>
    [HttpPost("invites")]
    [Authorize]
    [ProducesResponseType(typeof(LeagueInviteCreatedDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeagueInviteCreatedDTO>> CreateInvite(int leagueId, [FromBody] LeagueInviteCreateDTO dto, CancellationToken ct)
    {
        var league = await leagues.GetByIdAsync(leagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        var creator = auth.GetUserId(User) ?? throw new UnauthorizedAccessException("Authentication is required.");
        var created = await service.CreateInviteAsync(leagueId, creator, dto.ExpiresInHours, ct);
        return StatusCode(StatusCodes.Status201Created, new LeagueInviteCreatedDTO(created.Token, created.CreatedAt, created.ExpiresAt));
    }

    /// <summary>Lists invite metadata without exposing token values.</summary>
    [HttpGet("invites")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<LeagueInviteReadDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<LeagueInviteReadDTO>>> GetInvites(int leagueId, CancellationToken ct)
    {
        var league = await leagues.GetByIdAsync(leagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        var invites = await service.GetInvitesAsync(leagueId, ct);
        return Ok(invites.Select(x => new LeagueInviteReadDTO(x.LeagueInviteId, x.CreatedAt, x.ExpiresAt, x.UsedAt, x.UsedByUserId, x.RevokedAt)).ToArray());
    }

    /// <summary>Revokes a league invite.</summary>
    [HttpDelete("invites/{inviteId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeInvite(int leagueId, int inviteId, CancellationToken ct)
    {
        var league = await leagues.GetByIdAsync(leagueId, ct);
        if (!auth.HasAccessToResource(league.OwnerUserId, User)) throw new ForbiddenException("You do not have access to this resource.");
        await service.RevokeInviteAsync(leagueId, inviteId, ct);
        return NoContent();
    }

    /// <summary>Removes a member from the league.</summary>
    [HttpDelete("members/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int leagueId, int id, CancellationToken ct)
    {
        var registration = await service.GetByIdAsync(id, ct);
        if (registration.LeagueId != leagueId) throw new IRacingLeague.Business.NotFoundException($"Member {id} was not found in league {leagueId}.");
        var league = await leagues.GetByIdAsync(leagueId, ct); var self = auth.GetUserId(User) == registration.UserId; var owner = auth.HasAccessToResource(league.OwnerUserId, User);
        if (!self && !owner) throw new ForbiddenException("You do not have access to this resource.");
        await service.DeleteAsync(id, ct); return NoContent();
    }
}
