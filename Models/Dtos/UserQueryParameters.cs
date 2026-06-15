namespace IRacingLeague.Models.Dtos;

public class UserQueryParameters
{
    public int? MinIRating { get; set; }

    public string? LicenseClass { get; set; }

    public string? SortBy { get; set; }

    public bool Desc { get; set; }
}
