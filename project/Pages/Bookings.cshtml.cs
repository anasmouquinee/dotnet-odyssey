using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Models;
using project.Services;

namespace project.Pages;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class BookingsModel : PageModel
{
    private readonly IBookingService _bookingService;
    
    public BookingsModel(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }
    
    public List<Booking> Bookings { get; set; } = new();
    public string? SuccessMessage { get; set; }
    
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }
    
    public async Task OnGetAsync(string? message = null)
    {
        SuccessMessage = message;
        var userId = GetUserId();
        Bookings = await _bookingService.GetUserBookingsAsync(userId);
    }
    
    public async Task<IActionResult> OnPostCancelAsync(int bookingId)
    {
        var userId = GetUserId();
        await _bookingService.CancelBookingAsync(bookingId, userId);
        return RedirectToPage();
    }
}
