using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Models;
using project.Services;

namespace project.Pages;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class ProfileModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IBookingService _bookingService;
    
    public ProfileModel(IAuthService authService, IBookingService bookingService)
    {
        _authService = authService;
        _bookingService = bookingService;
    }
    
    public Models.User UserProfile { get; set; } = null!;
    public List<Booking> Bookings { get; set; } = new();
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int PendingBookings { get; set; }
    public decimal TotalSpent { get; set; }
    
    private int GetUserId()
    {
        var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }
    
    public async Task OnGetAsync()
    {
        var userId = GetUserId();
        
        UserProfile = await _authService.GetUserByIdAsync(userId) ?? new Models.User();
        Bookings = await _bookingService.GetUserBookingsAsync(userId);
        
        TotalBookings = Bookings.Count;
        CompletedBookings = Bookings.Count(b => b.Status == BookingStatus.Completed);
        PendingBookings = Bookings.Count(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed);
        TotalSpent = Bookings.Where(b => b.Status != BookingStatus.Cancelled).Sum(b => b.TotalPrice);
    }
}
