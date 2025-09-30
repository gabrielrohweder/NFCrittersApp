namespace AnimalCollector.Shared.DTOs;

public class AnimalDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Habitat { get; set; } = string.Empty;
    public string Rarity { get; set; } = "common";
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> Facts { get; set; } = new();
    public bool Collected { get; set; }
}
