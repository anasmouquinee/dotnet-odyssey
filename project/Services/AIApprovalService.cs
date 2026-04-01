using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;
using System.Text.Json;
using System.Text;

namespace project.Services;

public interface IAIApprovalService
{
    Task<int> ProcessPendingBookingsAsync();
}

public class AIApprovalService : IAIApprovalService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly IAdminNotificationService _notificationService;
    private readonly ILogger<AIApprovalService> _logger;

    public AIApprovalService(
        ApplicationDbContext context, 
        IConfiguration config, 
        HttpClient httpClient, 
        IAdminNotificationService notificationService,
        ILogger<AIApprovalService> logger)
    {
        _context = context;
        _config = config;
        _httpClient = httpClient;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<int> ProcessPendingBookingsAsync()
    {
        var pendingBookings = await _context.Bookings
            .Include(b => b.TravelPackage)
            .Include(b => b.User)
            .Where(b => b.Status == BookingStatus.Pending)
            .ToListAsync();

        int processedCount = 0;

        foreach (var booking in pendingBookings)
        {
            var (isApproved, aiReason) = await EvaluateBookingWithAIAsync(booking);

            if (isApproved)
            {
                booking.Status = BookingStatus.Confirmed;
                booking.ConfirmedAt = DateTime.UtcNow;
                
                await _notificationService.NotifyAdminAsync(
                    "🤖 AI Approved Booking", 
                    $"Booking #{booking.Id} for {booking.User.FirstName} to {booking.TravelPackage.Destination} has been automatically approved.\n\nAI Notes: {aiReason}",
                    "22c55e" // Green
                );
            }
            else
            {
                // Put it back to pending or cancel it. Let's mark as Cancelled or left for human review.
                // We will cancel it if AI strictly rejects it.
                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;

                await _notificationService.NotifyAdminAsync(
                    "🛑 AI Rejected Booking", 
                    $"Booking #{booking.Id} for {booking.User.FirstName} to {booking.TravelPackage.Destination} was rejected by the AI.\n\nReason: {aiReason}",
                    "ef4444" // Red
                );
            }

            processedCount++;
        }

        if (processedCount > 0)
        {
            await _context.SaveChangesAsync();
        }

        return processedCount;
    }

    private async Task<(bool IsApproved, string Reason)> EvaluateBookingWithAIAsync(Booking booking)
    {
        var apiKey = _config["Gemini:ApiKey"];

        // If no API key is provided, Fallback to a smart Rule-Based "AI" Simulator
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("No Gemini API Key found. Using fallback rule-based evaluation.");
            return SimulateAIEvaluation(booking);
        }

        var prompt = $@"
        You are a luxury travel concierge acting as an auto-approver.
        Evaluate this booking request:
        - Destination: {booking.TravelPackage.Destination}
        - Season: {booking.TravelPackage.Season}
        - Trip Dates: {booking.StartDate:MMM dd, yyyy} to {booking.EndDate:MMM dd, yyyy}
        - Guests: {booking.NumberOfGuests}
        - Special Requests: {booking.SpecialRequests ?? "None"}

        Rules:
        1. Ensure the dates generally align with the season or aren't completely unreasonable (e.g. Winter in July for Europe). It's flexible, but major mismatches must be flagged.
        2. If special requests contain dangerous, illegal, or obviously impossible requests (e.g., 'I want a pet dinosaur'), reject it.
        3. Explain your reasoning briefly.

        Respond strictly in this JSON format:
        {{
            ""approved"": true/false,
            ""reason"": ""A brief 1-2 sentence explanation""
        }}";

        try
        {
            var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = "You are a backend JSON AI acting as an approval system. Only reply in valid JSON.\n\n" + prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2
                }
            };

            var response = await _httpClient.PostAsJsonAsync(requestUrl, payload);
            response.EnsureSuccessStatusCode();

            var responseStr = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseStr);
            var content = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (content != null)
            {
                // Gemini sometimes wraps JSON in markdown block ```json ... ```
                var cleanedContent = content.Trim();
                if (cleanedContent.StartsWith("```json"))
                {
                    cleanedContent = cleanedContent.Substring(7, cleanedContent.Length - 10).Trim();
                }
                else if (cleanedContent.StartsWith("```"))
                {
                    cleanedContent = cleanedContent.Substring(3, cleanedContent.Length - 6).Trim();
                }

                var resultDoc = JsonDocument.Parse(cleanedContent);
                bool approved = resultDoc.RootElement.GetProperty("approved").GetBoolean();
                string reason = resultDoc.RootElement.GetProperty("reason").GetString() ?? "";

                return (approved, reason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API. Falling back to default rules.");
        }

        // Fallback if API fails
        return SimulateAIEvaluation(booking);
    }

    private (bool, string) SimulateAIEvaluation(Booking booking)
    {
        // Simple heuristic rules if AI isn't connected
        var lowerRequests = booking.SpecialRequests?.ToLower() ?? "";
        
        if (lowerRequests.Contains("illegal") || lowerRequests.Contains("weapon") || booking.NumberOfGuests > 15)
        {
            return (false, "Request contains prohibited keywords or exceeds maximum standard group size.");
        }

        int duration = (booking.EndDate - booking.StartDate).Days;
        if (duration < 1 || duration > 60)
        {
            return (false, $"Trip duration of {duration} days is outside accepted parameters (1-60 days).");
        }

        return (true, "Automated check passed: No prohibited keywords, standardized party size, and typical duration.");
    }
}
