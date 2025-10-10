using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimalCollector.Shared.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string Username { get; set; } = string.Empty;

    public string? Password { get; set; }

    public string? Nickname { get; set; }

    public string AuthProvider { get; set; } = "local";

    public string? ExternalId { get; set; }

    public int Tokens { get; set; } = 0;

    public virtual ICollection<UserAnimal> UserAnimals { get; set; } = new List<UserAnimal>();
    
    public virtual ICollection<UserGift> UserGifts { get; set; } = new List<UserGift>();
}
