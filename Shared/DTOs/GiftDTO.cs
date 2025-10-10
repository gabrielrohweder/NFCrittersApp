namespace AnimalCollector.Shared.DTOs;

public class GiftDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool Boredom { get; set; } = false;
    public bool Hunger { get; set; } = false;
    public bool Sadness { get; set; } = false;
    public bool Health { get; set; } = false;
    public bool Energy { get; set; } = false;
}

