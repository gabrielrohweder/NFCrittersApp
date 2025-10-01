namespace AnimalCollector.Client.Models;

public class Achievement
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Func<int, int, int, int, bool> Condition { get; set; } = (_, _, _, _) => false;
}

public static class Achievements
{
    public static readonly Achievement FirstDiscovery = new()
    {
        Id = "first-discovery",
        Name = "First Discovery",
        Description = "Found your first animal!",
        Icon = "⭐",
        Condition = (collected, _, _, _) => collected == 1
    };

    public static readonly Achievement Collector = new()
    {
        Id = "collector",
        Name = "Collector",
        Description = "Collected 5 animals!",
        Icon = "⭐",
        Condition = (collected, _, _, _) => collected == 5
    };

    public static readonly Achievement Hunter = new()
    {
        Id = "hunter",
        Name = "Hunter",
        Description = "Collected 25 animals!",
        Icon = "⭐",
        Condition = (collected, _, _, _) => collected == 25
    };

    public static readonly Achievement LegendaryHunter = new()
    {
        Id = "legendary-hunter",
        Name = "Legendary Hunter",
        Description = "Found a legendary animal!",
        Icon = "⭐",
        Condition = (_, legendary, _, _) => legendary == 1
    };

    public static readonly Achievement Cryptozoologist = new()
    {
        Id = "cryptozoologist",
        Name = "Cryptozoologist",
        Description = "Discovered all legendary animals!",
        Icon = "🕵️",
        Condition = (_, legendary, totalLegendary, _) => legendary > 0 && legendary == totalLegendary
    };

    public static readonly Achievement Explorer = new()
    {
        Id = "explorer",
        Name = "Explorer",
        Description = "Discovered all species!",
        Icon = "⭐",
        Condition = (_, _, _, completion) => completion == 100
    };

    public static List<Achievement> All => new()
    {
        FirstDiscovery,
        Collector,
        Hunter,
        LegendaryHunter,
        Cryptozoologist,
        Explorer
    };
}
