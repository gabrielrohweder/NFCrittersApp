namespace AnimalCollector.Shared.DTOs;

public class AnimalMoodDTO
{
    public int Id { get; set; }
    public string AnimalId { get; set; } = string.Empty;
    public string AnimalName { get; set; } = string.Empty;
    public string AnimalImage { get; set; } = string.Empty;
    public string MoodState { get; set; } = "happy";
    public DateTime LastUpdated { get; set; }
}

public class UnhappyAnimalResponse
{
    public bool HasUnhappyAnimal { get; set; }
    public AnimalMoodDTO? UnhappyAnimal { get; set; }
    public bool HasGifts { get; set; }
    public bool CanAffordGift { get; set; }
    public int CheapestGiftPrice { get; set; }
}

public class ResolveMoodRequest
{
    public int MoodId { get; set; }
    public int? GiftId { get; set; }
    public bool PlayedGame { get; set; }
}

public class ResolveMoodResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RemainingTokens { get; set; }
}
