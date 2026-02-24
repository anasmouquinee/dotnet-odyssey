using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Models;
using project.Services;

namespace project.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITravelPackageService _packageService;
        private readonly ICartService _cartService;
        
        public IndexModel(ITravelPackageService packageService, ICartService cartService)
        {
            _packageService = packageService;
            _cartService = cartService;
        }
        
        public List<TravelPackage> TravelPackages { get; set; } = new();
        public int CartCount { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsAdmin { get; set; }
        public string? UserName { get; set; }
        
        public async Task OnGetAsync()
        {
            TravelPackages = await _packageService.GetAllPackagesAsync();
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
            
            if (IsAuthenticated)
            {
                UserName = User.FindFirst("FirstName")?.Value ?? User.Identity?.Name;
                IsAdmin = User.FindFirst("IsAdmin")?.Value == "True";
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    CartCount = await _cartService.GetCartCountAsync(userId);
                }
            }
        }
    }
}
