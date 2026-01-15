using System.Text;
using System.Text.Json;

namespace AnimalCollector.Server.Services;

public class EmailService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _httpClient = new HttpClient();
        _logger = logger;
    }

    private async Task<(string apiKey, string fromEmail)> GetResendCredentials()
    {
        var hostname = Environment.GetEnvironmentVariable("REPLIT_CONNECTORS_HOSTNAME");
        var replIdentity = Environment.GetEnvironmentVariable("REPL_IDENTITY");
        var webReplRenewal = Environment.GetEnvironmentVariable("WEB_REPL_RENEWAL");

        string? xReplitToken = null;
        if (!string.IsNullOrEmpty(replIdentity))
        {
            xReplitToken = "repl " + replIdentity;
        }
        else if (!string.IsNullOrEmpty(webReplRenewal))
        {
            xReplitToken = "depl " + webReplRenewal;
        }

        if (string.IsNullOrEmpty(xReplitToken))
        {
            throw new Exception("X_REPLIT_TOKEN not found for repl/depl");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, 
            $"https://{hostname}/api/v2/connection?include_secrets=true&connector_names=resend");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("X_REPLIT_TOKEN", xReplitToken);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        var items = json.RootElement.GetProperty("items");
        if (items.GetArrayLength() == 0)
        {
            throw new Exception("Resend not connected");
        }

        var settings = items[0].GetProperty("settings");
        var apiKey = settings.GetProperty("api_key").GetString() ?? throw new Exception("API key not found");
        var fromEmail = settings.GetProperty("from_email").GetString() ?? "noreply@resend.dev";

        return (apiKey, fromEmail);
    }

    public async Task<bool> SendPasswordResetEmail(string toEmail, string resetLink)
    {
        try
        {
            var (apiKey, fromEmail) = await GetResendCredentials();

            var emailRequest = new
            {
                from = fromEmail,
                to = new[] { toEmail },
                subject = "Reset Your Yume Zoo Password",
                html = $@"
                    <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
                        <h1 style=""color: #7c3aed;"">Reset Your Password</h1>
                        <p>We received a request to reset your password for your Yume Zoo account.</p>
                        <p>Click the button below to set a new password. This link will expire in 1 hour.</p>
                        <a href=""{resetLink}"" 
                           style=""display: inline-block; padding: 12px 24px; background-color: #7c3aed; 
                                  color: white; text-decoration: none; border-radius: 8px; margin: 20px 0;"">
                            Reset Password
                        </a>
                        <p style=""color: #666; font-size: 14px;"">
                            If you didn't request a password reset, you can ignore this email.
                        </p>
                        <p style=""color: #666; font-size: 14px;"">
                            If the button doesn't work, copy and paste this link into your browser:<br/>
                            <a href=""{resetLink}"">{resetLink}</a>
                        </p>
                    </div>
                "
            };

            var jsonContent = JsonSerializer.Serialize(emailRequest);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = httpContent;

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Password reset email sent to {Email}", toEmail);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email: {Error}", error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email");
            return false;
        }
    }
}
