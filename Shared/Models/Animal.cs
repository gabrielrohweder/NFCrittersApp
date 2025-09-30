using System.ComponentModel.DataAnnotations;

namespace AnimalCollector.Shared.Models;

public class Animal
{
    [Key]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Species { get; set; } = string.Empty;

    public string Habitat { get; set; } = string.Empty;

    [Required]
    public string Rarity { get; set; } = "common";

    public string ImageUrl { get; set; } = string.Empty;

    public string Facts { get; set; } = string.Empty; // JSON string array

    [Required]
    public string Token { get; set; } = string.Empty; // Unique token for NFC scanning

    public virtual ICollection<UserAnimal> UserAnimals { get; set; } = new List<UserAnimal>();
}
