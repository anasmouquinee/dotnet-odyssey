using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Models;
using project.Services;

namespace project.Pages.Admin;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class UsersModel : PageModel
{
    private readonly IAdminService _adminService;
    
    public UsersModel(IAdminService adminService)
    {
        _adminService = adminService;
    }
    
    public List<User> Users { get; set; } = new();
    public string? SuccessMessage { get; set; }
    
    public async Task<IActionResult> OnGetAsync()
    {
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
        if (!isAdmin) return RedirectToPage("/Account/AccessDenied");
        
        Users = await _adminService.GetAllUsersAsync();
        SuccessMessage = TempData["SuccessMessage"]?.ToString();
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostToggleAdminAsync(int userId)
    {
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
        if (!isAdmin) return RedirectToPage("/Account/AccessDenied");
        
        await _adminService.ToggleAdminStatusAsync(userId);
        TempData["SuccessMessage"] = "User role updated successfully";
        
        return RedirectToPage();
    }
}
