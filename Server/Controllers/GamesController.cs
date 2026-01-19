using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnimalCollector.Server.Data;
using AnimalCollector.Shared.DTOs;
using System.Collections.Concurrent;

namespace AnimalCollector.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private static readonly int[] WheelValues = { 1, 5, 10, 25, 50, 75, 100 };
    private static readonly int[] WheelWeights = { 30, 25, 20, 12, 8, 4, 1 }; // Higher weights for lower values
    private static readonly ConcurrentDictionary<string, DateTime> LastSpinAwardTime = new();
    private static readonly TimeSpan SpinAwardCooldown = TimeSpan.FromSeconds(30); // 30 second cooldown between awards

    public GamesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("award-wheelspin")]
    public async Task<ActionResult> AwardWheelSpin()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Rate limiting: prevent spam by enforcing cooldown between awards
        if (LastSpinAwardTime.TryGetValue(userId, out var lastAward))
        {
            if (DateTime.UtcNow - lastAward < SpinAwardCooldown)
            {
                return BadRequest(new { message = "Please wait before claiming another spin" });
            }
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.WheelSpins += 1;
        await _context.SaveChangesAsync();

        // Update last award time
        LastSpinAwardTime[userId] = DateTime.UtcNow;

        return Ok(new { wheelSpins = user.WheelSpins });
    }

    [HttpPost("use-wheelspin")]
    public async Task<ActionResult> UseWheelSpin()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        if (user.WheelSpins <= 0)
        {
            return BadRequest(new { message = "No wheel spins available" });
        }

        // Generate tokens server-side using weighted random
        var tokensWon = GetWeightedRandomValue();

        user.WheelSpins -= 1;
        user.Tokens += tokensWon;
        await _context.SaveChangesAsync();

        return Ok(new { 
            wheelSpins = user.WheelSpins, 
            tokens = user.Tokens,
            tokensWon = tokensWon 
        });
    }

    private static int GetWeightedRandomValue()
    {
        var totalWeight = WheelWeights.Sum();
        var randomValue = Random.Shared.Next(totalWeight);
        
        var cumulativeWeight = 0;
        for (int i = 0; i < WheelValues.Length; i++)
        {
            cumulativeWeight += WheelWeights[i];
            if (randomValue < cumulativeWeight)
            {
                return WheelValues[i];
            }
        }
        
        return WheelValues[0]; // Fallback
    }

    [HttpGet("wheelspin-count")]
    public async Task<ActionResult> GetWheelSpinCount()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new { wheelSpins = user.WheelSpins });
    }
}
