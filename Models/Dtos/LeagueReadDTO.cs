namespace IRacingLeague.Models.Dtos;

public class LeagueReadDTO
{
    public int LeagueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Discipline { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int MaxDrivers { get; set; }
    public decimal EntryFee { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OwnerUserId { get; set; }

    public static LeagueReadDTO FromEntity(League league) => new()
    {
        LeagueId = league.LeagueId,
        Name = league.Name,
        Discipline = league.Discipline,
        IsPublic = league.IsPublic,
        MaxDrivers = league.MaxDrivers,
        EntryFee = league.EntryFee,
        CreatedAt = league.CreatedAt,
        OwnerUserId = league.OwnerUserId
    };
}
