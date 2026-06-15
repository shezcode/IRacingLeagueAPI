using System.ComponentModel.DataAnnotations;

namespace IRacingLeague.Models.Dtos;

public class RaceCreateDTO
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Track { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string Car { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduledAt { get; set; }

    [Range(1, 500)]
    public int LapCount { get; set; }

    [Range(-20, 60)]
    public decimal AmbientTempC { get; set; }
}
