using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnimalCollector.Server.Data;
using AnimalCollector.Server.Helpers;
using AnimalCollector.Shared.Models;
using AnimalCollector.Shared.DTOs;

namespace AnimalCollector.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var normalizedUsername = request.Username.ToLower();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername);

        if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.Password))
        {
            return Ok(new AuthResponse 
            { 
                Success = false, 
                Message = "Invalid username or password" 
            });
        }

        HttpContext.Session.SetString("UserId", user.Id);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            User = new UserDTO
            {
                Id = user.Id,
                Username = user.Username
            }
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var normalizedUsername = request.Username.ToLower();
        
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername))
        {
            return Ok(new AuthResponse 
            { 
                Success = false, 
                Message = "Username already exists" 
            });
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = normalizedUsername,
            Password = PasswordHelper.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        HttpContext.Session.SetString("UserId", user.Id);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Registration successful",
            User = new UserDTO
            {
                Id = user.Id,
                Username = user.Username
            }
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new AuthResponse 
        { 
            Success = true, 
            Message = "Logged out successfully" 
        });
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDTO?>> GetCurrentUser()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return Ok(null);
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return Ok(null);
        }

        return Ok(new UserDTO
        {
            Id = user.Id,
            Username = user.Username
        });
    }
}
