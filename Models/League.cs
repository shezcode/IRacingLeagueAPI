namespace IRacingLeague.Models;

public class League
{
    public int LeagueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Discipline { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int MaxDrivers { get; set; }
    public decimal EntryFee { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OwnerUserId { get; set; }
    public uint Version { get; set; }
    public int[] PointsTable { get; set; } = [25, 18, 15, 12, 10, 8, 6, 4, 2, 1];
    public int FastestLapBonus { get; set; } = 1;

    // Parameterless ctor kept for JSON (de)serialization in a later step.
    public League() { }

    public League(string name, string discipline, bool isPublic, int maxDrivers, decimal entryFee, int ownerUserId)
    {
        Name = name;
        Discipline = discipline;
        IsPublic = isPublic;
        MaxDrivers = maxDrivers;
        EntryFee = entryFee;
        OwnerUserId = ownerUserId;
        CreatedAt = DateTime.UtcNow;
    }

}
