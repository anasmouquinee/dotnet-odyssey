using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Services;

namespace project.Pages.Api;

[Authorize(AuthenticationSchemes = "CookieAuth")]
[ValidateAntiForgeryToken]
public class AddToCartModel : PageModel
{
    private readonly ICartService _cartService;
    private readonly ILogger<AddToCartModel> _logger;
    
    public AddToCartModel(ICartService cartService, ILogger<AddToCartModel> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }
    
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }
    
    public IActionResult OnGet()
    {
        return RedirectToPage("/Index");
    }
    
    public async Task<IActionResult> OnPostAsync(
        int packageId, 
        DateTime startDate, 
        DateTime endDate, 
        int guests, 
        string? specialRequests)
    {
        try
        {
            var userId = GetUserId();
            
            if (userId == 0)
            {
                return new JsonResult(new { success = false, message = "Please login first", redirect = "/Account/Login" });
            }
            
            _logger.LogInformation("Adding to cart: PackageId={PackageId}, UserId={UserId}, StartDate={StartDate}, EndDate={EndDate}, Guests={Guests}", 
                packageId, userId, startDate, endDate, guests);
            
            var cartItem = await _cartService.AddToCartAsync(userId, packageId, startDate, endDate, guests, specialRequests);
            
            if (cartItem == null)
            {
                _logger.LogWarning("Package not found: PackageId={PackageId}", packageId);
                return new JsonResult(new { success = false, message = "Package not found" });
            }
            
            var cartCount = await _cartService.GetCartCountAsync(userId);
            
            _logger.LogInformation("Successfully added to cart. CartCount={CartCount}", cartCount);
            
            return new JsonResult(new { success = true, message = "Added to cart!", cartCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to cart: PackageId={PackageId}", packageId);
            return new JsonResult(new { success = false, message = "An error occurred while adding to cart" });
        }
    }
}
