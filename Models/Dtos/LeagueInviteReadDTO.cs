namespace IRacingLeague.Models.Dtos;

public sealed record LeagueInviteReadDTO(int Id, DateTime CreatedAt, DateTime ExpiresAt, DateTime? UsedAt, int? UsedByUserId, DateTime? RevokedAt);
