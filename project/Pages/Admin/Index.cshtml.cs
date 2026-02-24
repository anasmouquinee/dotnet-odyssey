using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Services;

namespace project.Pages.Admin;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class IndexModel : PageModel
{
    private readonly IAdminService _adminService;
    
    public IndexModel(IAdminService adminService)
    {
        _adminService = adminService;
    }
    
    public AdminDashboardStats Stats { get; set; } = new();
    
    public async Task<IActionResult> OnGetAsync()
    {
        // Check if user is admin
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
        if (!isAdmin)
        {
            return RedirectToPage("/Account/AccessDenied");
        }
        
        Stats = await _adminService.GetDashboardStatsAsync();
        return Page();
    }
}
