using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Models;
using project.Services;

namespace project.Pages;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class CartModel : PageModel
{
    private readonly ICartService _cartService;
    private readonly IBookingService _bookingService;
    
    public CartModel(ICartService cartService, IBookingService bookingService)
    {
        _cartService = cartService;
        _bookingService = bookingService;
    }
    
    public List<CartItem> CartItems { get; set; } = new();
    public decimal CartTotal { get; set; }
    
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }
    
    public async Task OnGetAsync()
    {
        var userId = GetUserId();
        CartItems = await _cartService.GetCartItemsAsync(userId);
        CartTotal = await _cartService.GetCartTotalAsync(userId);
    }
    
    public async Task<IActionResult> OnPostRemoveAsync(int cartItemId)
    {
        var userId = GetUserId();
        await _cartService.RemoveFromCartAsync(cartItemId, userId);
        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostCheckoutAsync()
    {
        var userId = GetUserId();
        var bookings = await _bookingService.CheckoutCartAsync(userId);
        
        if (bookings.Any())
        {
            return RedirectToPage("/Bookings", new { message = "Your bookings have been confirmed!" });
        }
        
        return RedirectToPage();
    }
}
