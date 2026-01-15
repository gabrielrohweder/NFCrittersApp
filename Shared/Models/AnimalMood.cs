using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimalCollector.Shared.Models;

public class AnimalMood
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [Required]
    public string AnimalId { get; set; } = string.Empty;

    [ForeignKey(nameof(AnimalId))]
    public virtual Animal Animal { get; set; } = null!;

    [Required]
    public string MoodState { get; set; } = "happy";

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
