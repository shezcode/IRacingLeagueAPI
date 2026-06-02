using System.ComponentModel.DataAnnotations;

namespace IRacingLeague.Models.Dtos;

public class LeagueCreateDTO
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Discipline { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    [Range(1, 100)]
    public int MaxDrivers { get; set; }

    [Range(0, 100000)]
    public decimal EntryFee { get; set; }

    // Owner is NOT taken from the body, the creating Driver becomes the owner, read
    // from the JWT identity in the controller. Kept off the DTO so a caller
    // can't create a league owned by someone else.
}
