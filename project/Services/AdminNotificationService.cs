using System.Text;
using System.Text.Json;

namespace project.Services;

public interface IAdminNotificationService
{
    Task NotifyAdminAsync(string title, string message, string colorHex = "00ff00");
}

public class AdminNotificationService : IAdminNotificationService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AdminNotificationService> _logger;

    public AdminNotificationService(IConfiguration config, HttpClient httpClient, ILogger<AdminNotificationService> logger)
    {
        _config = config;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task NotifyAdminAsync(string title, string message, string colorHex = "22c55e") // default to green
    {
        // 1. Webhook Notification (e.g. Discord or Slack)
        var webhookUrl = _config["AdminNotifications:WebhookUrl"];
        if (!string.IsNullOrEmpty(webhookUrl))
        {
            try
            {
                var payload = new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = title,
                            description = message,
                            color = Convert.ToInt32(colorHex.TrimStart('#'), 16)
                        }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                await _httpClient.PostAsync(webhookUrl, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send webhook notification.");
            }
        }

        // 2. Email Simulation (You can plug in standard SmtpClient or SendGrid here)
        _logger.LogInformation("EMAIL TO ADMIN ({Email}): [{Title}] {Message}", _config["AdminNotifications:Email"] ?? "admin@odyssey.com", title, message);
    }
}
