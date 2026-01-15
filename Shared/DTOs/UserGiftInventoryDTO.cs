namespace AnimalCollector.Shared.DTOs;

public class UserGiftInventoryDTO
{
    public int GiftId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
}
