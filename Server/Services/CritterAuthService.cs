using System.Security.Cryptography;
using System.Text;

namespace AnimalCollector.Server.Services;

public class CritterAuthService
{
    private readonly string _secret;

    public CritterAuthService(IConfiguration configuration)
    {
        _secret = configuration["CRITTER_AUTH_SECRET"] 
            ?? Environment.GetEnvironmentVariable("CRITTER_AUTH_SECRET") 
            ?? throw new InvalidOperationException("CRITTER_AUTH_SECRET is not configured");
    }

    public bool VerifyHash(string token, string? uid, string hash)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(hash))
        {
            return false;
        }

        var expectedHash = GenerateHash(token, uid ?? "");
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedHash.ToLowerInvariant()),
            Encoding.UTF8.GetBytes(hash.ToLowerInvariant())
        );
    }

    public string GenerateHash(string token, string uid)
    {
        var message = $"{token}:{uid ?? ""}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
