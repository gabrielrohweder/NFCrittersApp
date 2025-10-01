using System.Text.RegularExpressions;

namespace AnimalCollector.Server.Services;

public static class ContentFilter
{
    private static readonly string[] BadWords = new[]
    {
        "damn", "hell", "crap", "stupid", "dumb", "idiot", "hate", "kill",
        "ugly", "fat", "loser", "suck", "shut", "butt", "poop", "fart","nigger", "nazi", "fuck","fucker", "fucking",
        "asshole", "bitch", "shit", "damn", "penis", "vagina", "pussy", "cock", "tits", "boobs", "titties",
        "piss", "pissed", "pissing", "shit", "cunt", "nigga", "gook", "chink", "retard"
        // Add more as needed - keeping it child-appropriate
    };

    public static bool ContainsBadWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var lowerText = text.ToLower();
        
        foreach (var badWord in BadWords)
        {
            // Check for the bad word as a whole word or part of a word
            if (lowerText.Contains(badWord))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Simple email validation regex
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
