using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnimalCollector.Server.Data;
using AnimalCollector.Server.Helpers;
using AnimalCollector.Server.Services;
using AnimalCollector.Shared.Models;
using AnimalCollector.Shared.DTOs;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

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

    [HttpGet("external-login")]
    public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    [HttpGet("external-login-callback")]
    public async Task<IActionResult> ExternalLoginCallback([FromQuery] string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync("Cookies");
        if (!result.Succeeded)
        {
            return Redirect($"/#/auth?error=external_auth_failed");
        }

        var claims = result.Principal?.Claims;
        var nameIdentifier = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var provider = result.Properties?.Items[".AuthScheme"] ?? "Unknown";

        if (string.IsNullOrEmpty(nameIdentifier) || string.IsNullOrEmpty(email))
        {
            return Redirect($"/#/auth?error=missing_claims");
        }

        // Check if user exists with this external provider
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.AuthProvider == provider && u.ExternalId == nameIdentifier);

        if (user == null)
        {
            // Check if user exists with same email
            var normalizedEmail = email.ToLower();
            user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedEmail);

            if (user != null)
            {
                // Link existing account to external provider
                user.AuthProvider = provider;
                user.ExternalId = nameIdentifier;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Create new user
                var nickname = name?.Split(' ').FirstOrDefault() ?? email.Split('@').FirstOrDefault() ?? "Explorer";
                
                // Ensure nickname is unique
                var baseNickname = nickname;
                var counter = 1;
                while (await _context.Users.AnyAsync(u => u.Nickname != null && u.Nickname.ToLower() == nickname.ToLower()))
                {
                    nickname = $"{baseNickname}{counter}";
                    counter++;
                }

                user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = normalizedEmail,
                    Password = null,
                    Nickname = nickname,
                    AuthProvider = provider,
                    ExternalId = nameIdentifier
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
        }

        HttpContext.Session.SetString("UserId", user.Id);

        return Redirect(returnUrl ?? "/#/");
    }

    [HttpGet("providers")]
    public IActionResult GetAvailableProviders()
    {
        var schemes = HttpContext.RequestServices
            .GetRequiredService<IAuthenticationSchemeProvider>()
            .GetAllSchemesAsync()
            .Result;

        var providers = schemes
            .Where(s => s.DisplayName != null && s.Name != "Cookies")
            .Select(s => new { name = s.Name, displayName = s.DisplayName })
            .ToList();

        return Ok(providers);
    }
}
