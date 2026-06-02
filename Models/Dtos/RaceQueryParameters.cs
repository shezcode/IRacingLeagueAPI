namespace IRacingLeague.Models.Dtos;

public class RaceQueryParameters
{
    public string? Track { get; set; }

    public DateTime? ScheduledFrom { get; set; }

    public DateTime? ScheduledTo { get; set; }

    public string? SortBy { get; set; }

    public bool Desc { get; set; }
}
