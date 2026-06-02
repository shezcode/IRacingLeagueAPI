namespace IRacingLeague.Models.Dtos;

public class LeagueQueryParameters
{
    public string? Discipline { get; set; }

    public string? SortBy { get; set; }

    public bool Desc { get; set; }
}
