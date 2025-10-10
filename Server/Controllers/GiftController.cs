using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnimalCollector.Server.Data;
using AnimalCollector.Shared.DTOs;
using AnimalCollector.Shared.Models;

namespace AnimalCollector.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GiftController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GiftController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<GiftDTO>>> GetGifts()
    {
        var gifts = await _context.Gifts.ToListAsync();

        var giftDTOs = gifts.Select(g => new GiftDTO
        {
            Id = g.Id.ToString(),
            Name = g.Name,
            Price = g.Price,
            ImageUrl = g.Image,
            Boredom = g.Boredom,
            Hunger = g.Hunger,
            Sadness = g.Sadness,
            Health = g.Health,
            Energy = g.Energy
        }).ToList();

        return Ok(giftDTOs);
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseGift([FromBody] PurchaseGiftRequest request)
    {
        var userId = HttpContext.Session.GetString("UserId");
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Please log in to purchase gifts" });
        }

        // Get the gift
        var gift = await _context.Gifts.FindAsync(request.GiftId);
        if (gift == null)
        {
            return NotFound(new { message = "Gift not found" });
        }

        // Get the user
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Check if user has enough tokens
        if (user.Tokens < gift.Price)
        {
            return BadRequest(new { message = $"Not enough tokens. You have {user.Tokens} tokens but need {gift.Price}" });
        }

        // Deduct tokens
        user.Tokens -= gift.Price;

        // Check if user already owns this gift
        var existingUserGift = await _context.UserGifts
            .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GiftId == request.GiftId);

        if (existingUserGift != null)
        {
            // Increment quantity
            existingUserGift.Quantity += 1;
        }
        else
        {
            // Create new user gift
            var userGift = new UserGift
            {
                UserId = userId,
                GiftId = request.GiftId,
                Quantity = 1,
                PurchasedAt = DateTime.UtcNow
            };
            _context.UserGifts.Add(userGift);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"Successfully purchased {gift.Name}!",
            remainingTokens = user.Tokens,
            giftName = gift.Name
        });
    }
}

public class PurchaseGiftRequest
{
    public int GiftId { get; set; }
}
