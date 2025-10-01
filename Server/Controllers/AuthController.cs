using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnimalCollector.Server.Data;
using AnimalCollector.Server.Helpers;
using AnimalCollector.Server.Services;
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
                Username = user.Username,
                Nickname = user.Nickname ?? string.Empty
            }
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        // Validate email format
        if (!ContentFilter.IsValidEmail(request.Username))
        {
            return Ok(new AuthResponse 
            { 
                Success = false, 
                Message = "Please use a valid email address as your username" 
            });
        }

        // Check for inappropriate content in email
        if (ContentFilter.ContainsBadWords(request.Username))
        {
            return Ok(new AuthResponse 
            { 
                Success = false, 
                Message = "Please choose an appropriate username" 
            });
        }

        // Validate nickname format
        if (!ContentFilter.IsValidNickname(request.Nickname))
        {
            return Ok(new AuthResponse 
            { 
                Success = false, 
                Message = "Nickname must be 3-20 characters and can only contain letters, numbers, spaces, underscores, and hyphens" 
            });
        }

        // Check for inappropriate content in nickname
        if (ContentFilter.ContainsBadWords(request.Nickname))
        {
            return Ok(new AuthResponse 
            { 
                Success = false, 
                Message = "Please choose an appropriate nickname" 
            });
        }

        var normalizedUsername = request.Username.ToLower();
        var normalizedNickname = ContentFilter.NormalizeNickname(request.Nickname);
        
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername))
        {
            return Ok(new AuthResponse 
            { 
                Success = false, 
                Message = "Email already exists" 
            });
        }

        // Check for unique nickname (case-insensitive)
        if (await _context.Users.AnyAsync(u => u.Nickname != null && u.Nickname.ToLower() == normalizedNickname.ToLower()))
        {
            return Ok(new AuthResponse 
            { 
                Success = false, 
                Message = "Nickname already taken" 
            });
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = normalizedUsername,
            Password = PasswordHelper.HashPassword(request.Password),
            Nickname = normalizedNickname
        };

        _context.Users.Add(user);
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Handle race condition where nickname was taken between check and insert
            return Ok(new AuthResponse 
            { 
                Success = false, 
                Message = "Nickname already taken. Please try a different one." 
            });
        }

        HttpContext.Session.SetString("UserId", user.Id);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Registration successful",
            User = new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = user.Nickname ?? string.Empty
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
            Username = user.Username,
            Nickname = user.Nickname ?? string.Empty
        });
    }
}
