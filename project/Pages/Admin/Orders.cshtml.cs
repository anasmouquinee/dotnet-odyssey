using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Models;
using project.Services;

namespace project.Pages.Admin;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class OrdersModel : PageModel
{
    private readonly IAdminService _adminService;
    
    public OrdersModel(IAdminService adminService)
    {
        _adminService = adminService;
    }
    
    public List<Booking> Bookings { get; set; } = new();
    public List<Booking> FilteredBookings { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }
    
    public string? SuccessMessage { get; set; }
    
    public async Task<IActionResult> OnGetAsync()
    {
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
        if (!isAdmin) return RedirectToPage("/Account/AccessDenied");
        
        Bookings = await _adminService.GetAllBookingsAsync();
        
        FilteredBookings = StatusFilter?.ToLower() switch
        {
            "pending" => Bookings.Where(b => b.Status == BookingStatus.Pending).ToList(),
            "confirmed" => Bookings.Where(b => b.Status == BookingStatus.Confirmed).ToList(),
            "cancelled" => Bookings.Where(b => b.Status == BookingStatus.Cancelled).ToList(),
            "completed" => Bookings.Where(b => b.Status == BookingStatus.Completed).ToList(),
            _ => Bookings
        };
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostUpdateStatusAsync(int bookingId, string status)
    {
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
        if (!isAdmin) return RedirectToPage("/Account/AccessDenied");
        
        if (Enum.TryParse<BookingStatus>(status, out var bookingStatus))
        {
            await _adminService.UpdateBookingStatusAsync(bookingId, bookingStatus);
            TempData["SuccessMessage"] = $"Order #{bookingId} has been updated to {status}";
        }
        
        return RedirectToPage(new { status = StatusFilter });
    }
}
