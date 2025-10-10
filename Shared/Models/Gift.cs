using System.ComponentModel.DataAnnotations;

namespace AnimalCollector.Shared.Models;

public class Gift
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Price { get; set; }

    public string Image { get; set; } = string.Empty;

    public bool Boredom { get; set; } = false;

    public bool Hunger { get; set; } = false;

    public bool Sadness { get; set; } = false;

    public bool Health { get; set; } = false;

    public bool Energy { get; set; } = false;

    public virtual ICollection<UserGift> UserGifts { get; set; } = new List<UserGift>();
}
