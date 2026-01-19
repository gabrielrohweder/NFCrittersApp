using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnimalCollector.Server.Data;
using AnimalCollector.Shared.DTOs;

namespace AnimalCollector.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

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

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.WheelSpins += 1;
        await _context.SaveChangesAsync();

        return Ok(new { wheelSpins = user.WheelSpins });
    }

    [HttpPost("use-wheelspin")]
    public async Task<ActionResult> UseWheelSpin([FromBody] UseWheelSpinRequest request)
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

        user.WheelSpins -= 1;
        user.Tokens += request.TokensWon;
        await _context.SaveChangesAsync();

        return Ok(new { 
            wheelSpins = user.WheelSpins, 
            tokens = user.Tokens,
            tokensWon = request.TokensWon 
        });
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

public class UseWheelSpinRequest
{
    public int TokensWon { get; set; }
}
