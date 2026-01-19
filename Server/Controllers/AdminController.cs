using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnimalCollector.Server.Data;
using AnimalCollector.Shared.Models;
using System.Text.Json;

namespace AnimalCollector.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<bool> IsAdmin()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId)) return false;
        
        var user = await _context.Users.FindAsync(userId);
        return user?.IsAdmin ?? false;
    }

    [HttpGet("check")]
    public async Task<ActionResult<bool>> CheckAdminStatus()
    {
        return Ok(await IsAdmin());
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDTO>>> GetUsers()
    {
        if (!await IsAdmin())
        {
            return Forbid();
        }

        var users = await _context.Users
            .Select(u => new AdminUserDTO
            {
                Id = u.Id,
                Username = u.Username,
                Nickname = u.Nickname ?? "",
                AuthProvider = u.AuthProvider,
                Tokens = u.Tokens,
                IsAdmin = u.IsAdmin,
                CollectedCount = _context.UserAnimals.Count(ua => ua.UserId == u.Id)
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPut("users/{id}")]
    public async Task<ActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        if (!await IsAdmin())
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (request.Nickname != null) user.Nickname = request.Nickname;
        if (request.Tokens.HasValue) user.Tokens = request.Tokens.Value;
        if (request.IsAdmin.HasValue) user.IsAdmin = request.IsAdmin.Value;

        await _context.SaveChangesAsync();
        return Ok(new { message = "User updated successfully" });
    }

    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        if (!await IsAdmin())
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "User deleted successfully" });
    }

    [HttpGet("animals")]
    public async Task<ActionResult<List<AdminAnimalDTO>>> GetAnimals()
    {
        if (!await IsAdmin())
        {
            return Forbid();
        }

        var animals = await _context.Animals
            .Select(a => new AdminAnimalDTO
            {
                Id = a.Id,
                Name = a.Name,
                Species = a.Species,
                Habitat = a.Habitat,
                Rarity = a.Rarity,
                ImageUrl = a.ImageUrl,
                Facts = a.Facts,
                Token = a.Token,
                CollectedByCount = _context.UserAnimals.Count(ua => ua.AnimalId == a.Id)
            })
            .ToListAsync();

        return Ok(animals);
    }

    [HttpPost("animals")]
    public async Task<ActionResult> CreateAnimal([FromBody] CreateAnimalRequest request)
    {
        if (!await IsAdmin())
        {
            return Forbid();
        }

        var existingToken = await _context.Animals.FirstOrDefaultAsync(a => a.Token == request.Token);
        if (existingToken != null)
        {
            return BadRequest(new { message = "An animal with this token already exists" });
        }

        var animal = new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Species = request.Species,
            Habitat = request.Habitat,
            Rarity = request.Rarity,
            ImageUrl = request.ImageUrl,
            Facts = JsonSerializer.Serialize(request.Facts ?? new List<string>()),
            Token = request.Token
        };

        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Animal created successfully", id = animal.Id });
    }

    [HttpPut("animals/{id}")]
    public async Task<ActionResult> UpdateAnimal(string id, [FromBody] UpdateAnimalRequest request)
    {
        if (!await IsAdmin())
        {
            return Forbid();
        }

        var animal = await _context.Animals.FindAsync(id);
        if (animal == null)
        {
            return NotFound();
        }

        if (request.Name != null) animal.Name = request.Name;
        if (request.Species != null) animal.Species = request.Species;
        if (request.Habitat != null) animal.Habitat = request.Habitat;
        if (request.Rarity != null) animal.Rarity = request.Rarity;
        if (request.ImageUrl != null) animal.ImageUrl = request.ImageUrl;
        if (request.Facts != null) animal.Facts = JsonSerializer.Serialize(request.Facts);
        if (request.Token != null) animal.Token = request.Token;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Animal updated successfully" });
    }

    [HttpDelete("animals/{id}")]
    public async Task<ActionResult> DeleteAnimal(string id)
    {
        if (!await IsAdmin())
        {
            return Forbid();
        }

        var animal = await _context.Animals.FindAsync(id);
        if (animal == null)
        {
            return NotFound();
        }

        _context.Animals.Remove(animal);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Animal deleted successfully" });
    }

    [HttpPost("animals/import")]
    public async Task<ActionResult> ImportAnimals([FromBody] List<CreateAnimalRequest> animals)
    {
        if (!await IsAdmin())
        {
            return Forbid();
        }

        var created = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var request in animals)
        {
            try
            {
                var existingToken = await _context.Animals.FirstOrDefaultAsync(a => a.Token == request.Token);
                if (existingToken != null)
                {
                    skipped++;
                    continue;
                }

                var animal = new Animal
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    Species = request.Species,
                    Habitat = request.Habitat,
                    Rarity = request.Rarity,
                    ImageUrl = request.ImageUrl,
                    Facts = JsonSerializer.Serialize(request.Facts ?? new List<string>()),
                    Token = request.Token
                };

                _context.Animals.Add(animal);
                created++;
            }
            catch (Exception ex)
            {
                errors.Add($"Error creating animal '{request.Name}': {ex.Message}");
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { 
            message = $"Import complete: {created} created, {skipped} skipped (duplicate tokens)",
            created,
            skipped,
            errors
        });
    }
}

public class AdminUserDTO
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string AuthProvider { get; set; } = string.Empty;
    public int Tokens { get; set; }
    public bool IsAdmin { get; set; }
    public int CollectedCount { get; set; }
}

public class UpdateUserRequest
{
    public string? Nickname { get; set; }
    public int? Tokens { get; set; }
    public bool? IsAdmin { get; set; }
}

public class AdminAnimalDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Habitat { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Facts { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public int CollectedByCount { get; set; }
}

public class CreateAnimalRequest
{
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Habitat { get; set; } = string.Empty;
    public string Rarity { get; set; } = "common";
    public string ImageUrl { get; set; } = string.Empty;
    public List<string>? Facts { get; set; }
    public string Token { get; set; } = string.Empty;
}

public class UpdateAnimalRequest
{
    public string? Name { get; set; }
    public string? Species { get; set; }
    public string? Habitat { get; set; }
    public string? Rarity { get; set; }
    public string? ImageUrl { get; set; }
    public List<string>? Facts { get; set; }
    public string? Token { get; set; }
}
