using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Models;
using project.Services;

namespace project.Pages.Bookings;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class EditModel : PageModel
{
    private readonly IBookingService _bookingService;
    
    public EditModel(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }
    
    public Booking? Booking { get; set; }
    
    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    public class InputModel
    {
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; }
        
        public string? SpecialRequests { get; set; }
    }
    
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }
    
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = GetUserId();
        Booking = await _bookingService.GetBookingByIdAsync(id, userId);
        
        if (Booking == null)
        {
            return NotFound();
        }
        
        Input = new InputModel
        {
            StartDate = Booking.StartDate,
            EndDate = Booking.EndDate,
            NumberOfGuests = Booking.NumberOfGuests,
            SpecialRequests = Booking.SpecialRequests
        };
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(int id)
    {
        var userId = GetUserId();
        Booking = await _bookingService.GetBookingByIdAsync(id, userId);
        
        if (Booking == null)
        {
            return NotFound();
        }
        
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        await _bookingService.UpdateBookingAsync(
            id, 
            userId, 
            Input.StartDate, 
            Input.EndDate, 
            Input.NumberOfGuests, 
            Input.SpecialRequests
        );
        
        return RedirectToPage("/Bookings", new { message = "Booking updated successfully!" });
    }
}
