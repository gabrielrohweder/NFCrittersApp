using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimalCollector.Shared.Models;

public class UserGift
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [Required]
    public int GiftId { get; set; }

    [ForeignKey(nameof(GiftId))]
    public virtual Gift Gift { get; set; } = null!;

    [Required]
    public int Quantity { get; set; } = 1;

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
}
