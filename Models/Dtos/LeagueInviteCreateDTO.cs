using System.ComponentModel.DataAnnotations;

namespace IRacingLeague.Models.Dtos;

public sealed class LeagueInviteCreateDTO
{
    [Range(1, 168)]
    public int ExpiresInHours { get; set; } = 72;
}
