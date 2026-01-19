using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnimalCollector.Server.Data;
using AnimalCollector.Server.Services;
using AnimalCollector.Shared.DTOs;
using AnimalCollector.Shared.Models;
using System.Text.Json;

namespace AnimalCollector.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly CritterAuthService _critterAuthService;

    public AnimalsController(ApplicationDbContext context, CritterAuthService critterAuthService)
    {
        _context = context;
        _critterAuthService = critterAuthService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AnimalDTO>>> GetAnimals()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var animals = await _context.Animals.ToListAsync();

        List<string> collectedAnimalIds = new();
        Dictionary<string, string> animalMoods = new();
        
        if (!string.IsNullOrEmpty(userId))
        {
            collectedAnimalIds = await _context.UserAnimals
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AnimalId)
                .ToListAsync();
            
            animalMoods = await _context.AnimalMoods
                .Where(m => m.UserId == userId && m.IsActive)
                .ToDictionaryAsync(m => m.AnimalId, m => m.MoodState);
        }

        var animalDTOs = animals.Select(a => new AnimalDTO
        {
            Id = a.Id,
            Name = a.Name,
            Species = a.Species,
            Habitat = a.Habitat,
            Rarity = a.Rarity,
            ImageUrl = a.ImageUrl,
            Facts = string.IsNullOrEmpty(a.Facts)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(a.Facts) ?? new List<string>(),
            Collected = collectedAnimalIds.Contains(a.Id),
            Mood = animalMoods.TryGetValue(a.Id, out var mood) ? mood : "Happy"
        }).ToList();

        return Ok(animalDTOs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnimalDTO>> GetAnimal(string id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var animal = await _context.Animals.FindAsync(id);

        if (animal == null)
        {
            return NotFound();
        }

        bool isCollected = false;
        if (!string.IsNullOrEmpty(userId))
        {
            isCollected = await _context.UserAnimals
                .AnyAsync(ua => ua.UserId == userId && ua.AnimalId == id);
        }

        var animalDTO = new AnimalDTO
        {
            Id = animal.Id,
            Name = animal.Name,
            Species = animal.Species,
            Habitat = animal.Habitat,
            Rarity = animal.Rarity,
            ImageUrl = animal.ImageUrl,
            Facts = string.IsNullOrEmpty(animal.Facts)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(animal.Facts) ?? new List<string>(),
            Collected = isCollected
        };

        return Ok(animalDTO);
    }


    [HttpGet("verify")]
    public async Task<ActionResult<AnimalDTO>> VerifyAndGetAnimal([FromQuery] string token, [FromQuery] string? uid, [FromQuery] string hash)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(hash))
        {
            return BadRequest(new { message = "Token and hash are required", authentic = false });
        }

        if (!_critterAuthService.VerifyHash(token, uid, hash))
        {
            return Unauthorized(new { message = "This critter is not authentic", authentic = false });
        }

        var userId = HttpContext.Session.GetString("UserId");
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Token == token);

        if (animal == null)
        {
            return NotFound(new { message = "Animal not found", authentic = true });
        }

        bool isCollected = false;
        string mood = "Happy";
        if (!string.IsNullOrEmpty(userId))
        {
            isCollected = await _context.UserAnimals
                .AnyAsync(ua => ua.UserId == userId && ua.AnimalId == animal.Id);
            
            var animalMood = await _context.AnimalMoods
                .FirstOrDefaultAsync(m => m.UserId == userId && m.AnimalId == animal.Id && m.IsActive);
            if (animalMood != null)
            {
                mood = animalMood.MoodState;
            }
        }

        var animalDTO = new AnimalDTO
        {
            Id = animal.Id,
            Name = animal.Name,
            Species = animal.Species,
            Habitat = animal.Habitat,
            Rarity = animal.Rarity,
            ImageUrl = animal.ImageUrl,
            Facts = string.IsNullOrEmpty(animal.Facts)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(animal.Facts) ?? new List<string>(),
            Collected = isCollected,
            Mood = mood
        };

        return Ok(new { animal = animalDTO, authentic = true });
    }

    [HttpPost("{id}/collect")]
    public async Task<IActionResult> CollectAnimal(string id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "You must be logged in to collect animals" });
        }

        var animal = await _context.Animals.FindAsync(id);
        if (animal == null)
        {
            return NotFound();
        }

        var exists = await _context.UserAnimals
            .AnyAsync(ua => ua.UserId == userId && ua.AnimalId == id);

        if (exists)
        {
            return Ok(new { message = "Animal already collected" });
        }

        var userAnimal = new UserAnimal
        {
            UserId = userId,
            AnimalId = id,
            CollectedAt = DateTime.UtcNow
        };

        _context.UserAnimals.Add(userAnimal);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Animal collected successfully" });
    }

    [HttpGet("collection")]
    public async Task<ActionResult<List<AnimalDTO>>> GetUserCollection()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var userAnimals = await _context.UserAnimals
            .Where(ua => ua.UserId == userId)
            .Include(ua => ua.Animal)
            .ToListAsync();

        var animalMoods = await _context.AnimalMoods
            .Where(m => m.UserId == userId && m.IsActive)
            .ToDictionaryAsync(m => m.AnimalId, m => m.MoodState);

        var animalDTOs = userAnimals.Select(ua => new AnimalDTO
        {
            Id = ua.Animal.Id,
            Name = ua.Animal.Name,
            Species = ua.Animal.Species,
            Habitat = ua.Animal.Habitat,
            Rarity = ua.Animal.Rarity,
            ImageUrl = ua.Animal.ImageUrl,
            Facts = string.IsNullOrEmpty(ua.Animal.Facts)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(ua.Animal.Facts) ?? new List<string>(),
            Collected = true,
            Mood = animalMoods.TryGetValue(ua.Animal.Id, out var mood) ? mood : "Happy"
        }).ToList();

        return Ok(animalDTOs);
    }

    [HttpGet("leaderboard")]
    public async Task<ActionResult<List<LeaderboardEntryDTO>>> GetLeaderboard()
    {
        var leaderboard = await _context.UserAnimals
            .GroupBy(ua => ua.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(3)
            .Join(
                _context.Users,
                ua => ua.UserId,
                u => u.Id,
                (ua, u) => new LeaderboardEntryDTO
                {
                    Nickname = u.Nickname ?? "Explorer",
                    CollectionCount = ua.Count
                })
            .ToListAsync();

        return Ok(leaderboard);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<StatsDTO>> GetStats()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var collectedAnimals = await _context.UserAnimals
            .Where(ua => ua.UserId == userId)
            .Join(_context.Animals,
                ua => ua.AnimalId,
                a => a.Id,
                (ua, a) => a)
            .ToListAsync();

        var totalAnimals = await _context.Animals.CountAsync();
        var totalLegendary = await _context.Animals.CountAsync(a => a.Rarity.ToLower() == "legendary");

        var stats = new StatsDTO
        {
            CollectedCount = collectedAnimals.Count,
            LegendaryCount = collectedAnimals.Count(a => a.Rarity.ToLower() == "legendary"),
            TotalLegendaryCount = totalLegendary,
            CompletionRate = totalAnimals > 0 ? (int)((double)collectedAnimals.Count / totalAnimals * 100) : 0
        };

        return Ok(stats);
    }

    [HttpGet("random")]
    public async Task<ActionResult<List<string>>> GetRandomAnimalImages([FromQuery] int count = 10)
    {
        var animals = await _context.Animals
            .OrderBy(a => Guid.NewGuid())
            .Take(count)
            .Select(a => a.ImageUrl)
            .ToListAsync();

        return Ok(animals);
    }
}
