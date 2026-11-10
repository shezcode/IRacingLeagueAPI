namespace IRacingLeague.Models;

public sealed class LeagueInvite
{
    public int LeagueInviteId { get; set; }
    public int LeagueId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public int? UsedByUserId { get; set; }
    public DateTime? RevokedAt { get; set; }
}
