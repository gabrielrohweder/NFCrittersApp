using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnimalCollector.Server.Data;
using AnimalCollector.Shared.DTOs;
using AnimalCollector.Shared.Models;

namespace AnimalCollector.Server.Controllers;

[ApiController]
[Route("api/moods")]
public class MoodsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private static readonly string[] UnhappyMoods = { "bored", "hungry", "scared", "angry" };
    private static readonly Random _random = new();

    public MoodsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("check")]
    public async Task<ActionResult<UnhappyAnimalResponse>> CheckMood()
    {
        var userId = HttpContext.Session.GetString("UserId");
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Please log in" });
        }

        var existingMood = await _context.AnimalMoods
            .Include(m => m.Animal)
            .Where(m => m.UserId == userId && m.IsActive && m.MoodState != "happy")
            .FirstOrDefaultAsync();

        if (existingMood != null)
        {
            var hasGifts = await _context.UserGifts
                .AnyAsync(ug => ug.UserId == userId && ug.Quantity > 0);

            var user = await _context.Users.FindAsync(userId);
            var cheapestGift = await _context.Gifts.MinAsync(g => g.Price);

            return Ok(new UnhappyAnimalResponse
            {
                HasUnhappyAnimal = true,
                UnhappyAnimal = new AnimalMoodDTO
                {
                    Id = existingMood.Id,
                    AnimalId = existingMood.AnimalId,
                    AnimalName = existingMood.Animal.Name,
                    AnimalImage = existingMood.Animal.ImageUrl ?? "",
                    MoodState = existingMood.MoodState,
                    LastUpdated = existingMood.LastUpdated
                },
                HasGifts = hasGifts,
                CanAffordGift = (user?.Tokens ?? 0) >= cheapestGift,
                CheapestGiftPrice = cheapestGift
            });
        }

        return Ok(new UnhappyAnimalResponse
        {
            HasUnhappyAnimal = false,
            UnhappyAnimal = null,
            HasGifts = false,
            CanAffordGift = false,
            CheapestGiftPrice = 0
        });
    }

    [HttpPost("roll")]
    public async Task<ActionResult<UnhappyAnimalResponse>> RollMood()
    {
        var userId = HttpContext.Session.GetString("UserId");
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Please log in" });
        }

        var existingActive = await _context.AnimalMoods
            .Where(m => m.UserId == userId && m.IsActive && m.MoodState != "happy")
            .FirstOrDefaultAsync();

        if (existingActive != null)
        {
            return await CheckMood();
        }

        if (_random.NextDouble() > 0.05)
        {
            return Ok(new UnhappyAnimalResponse
            {
                HasUnhappyAnimal = false,
                UnhappyAnimal = null,
                HasGifts = false,
                CanAffordGift = false,
                CheapestGiftPrice = 0
            });
        }

        var userAnimals = await _context.UserAnimals
            .Include(ua => ua.Animal)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();

        if (userAnimals.Count == 0)
        {
            return Ok(new UnhappyAnimalResponse
            {
                HasUnhappyAnimal = false,
                UnhappyAnimal = null,
                HasGifts = false,
                CanAffordGift = false,
                CheapestGiftPrice = 0
            });
        }

        var randomAnimal = userAnimals[_random.Next(userAnimals.Count)];
        var randomMood = UnhappyMoods[_random.Next(UnhappyMoods.Length)];

        var mood = new AnimalMood
        {
            UserId = userId,
            AnimalId = randomAnimal.AnimalId,
            MoodState = randomMood,
            LastUpdated = DateTime.UtcNow,
            IsActive = true
        };

        _context.AnimalMoods.Add(mood);
        await _context.SaveChangesAsync();

        var hasGifts = await _context.UserGifts
            .AnyAsync(ug => ug.UserId == userId && ug.Quantity > 0);

        var user = await _context.Users.FindAsync(userId);
        var cheapestGift = await _context.Gifts.MinAsync(g => g.Price);

        return Ok(new UnhappyAnimalResponse
        {
            HasUnhappyAnimal = true,
            UnhappyAnimal = new AnimalMoodDTO
            {
                Id = mood.Id,
                AnimalId = mood.AnimalId,
                AnimalName = randomAnimal.Animal.Name,
                AnimalImage = randomAnimal.Animal.ImageUrl ?? "",
                MoodState = mood.MoodState,
                LastUpdated = mood.LastUpdated
            },
            HasGifts = hasGifts,
            CanAffordGift = (user?.Tokens ?? 0) >= cheapestGift,
            CheapestGiftPrice = cheapestGift
        });
    }

    [HttpPost("resolve")]
    public async Task<ActionResult<ResolveMoodResponse>> ResolveMood([FromBody] ResolveMoodRequest request)
    {
        var userId = HttpContext.Session.GetString("UserId");
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Please log in" });
        }

        var mood = await _context.AnimalMoods
            .Include(m => m.Animal)
            .FirstOrDefaultAsync(m => m.Id == request.MoodId && m.UserId == userId && m.IsActive);

        if (mood == null)
        {
            return NotFound(new ResolveMoodResponse
            {
                Success = false,
                Message = "Mood not found or already resolved"
            });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new ResolveMoodResponse { Success = false, Message = "User not found" });
        }

        if (request.GiftId.HasValue)
        {
            var userGift = await _context.UserGifts
                .Include(ug => ug.Gift)
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GiftId == request.GiftId.Value && ug.Quantity > 0);

            if (userGift == null)
            {
                return BadRequest(new ResolveMoodResponse
                {
                    Success = false,
                    Message = "You don't have this gift in your inventory"
                });
            }

            userGift.Quantity -= 1;
            mood.MoodState = "happy";
            mood.IsActive = false;
            mood.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ResolveMoodResponse
            {
                Success = true,
                Message = $"You gave {mood.Animal.Name} a {userGift.Gift.Name} and they're happy now!",
                RemainingTokens = user.Tokens
            });
        }

        if (request.PlayedGame)
        {
            mood.MoodState = "happy";
            mood.IsActive = false;
            mood.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ResolveMoodResponse
            {
                Success = true,
                Message = $"{mood.Animal.Name} is happy now after you played with them!",
                RemainingTokens = user.Tokens
            });
        }

        return BadRequest(new ResolveMoodResponse
        {
            Success = false,
            Message = "Please provide a gift or play a game"
        });
    }
}
