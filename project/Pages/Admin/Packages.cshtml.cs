using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Models;
using project.Services;

namespace project.Pages.Admin;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class PackagesModel : PageModel
{
    private readonly IAdminService _adminService;
    
    public PackagesModel(IAdminService adminService)
    {
        _adminService = adminService;
    }
    
    public List<TravelPackage> Packages { get; set; } = new();
    public List<TravelPackage> FilteredPackages { get; set; } = new();
    
    [BindProperty(SupportsGet = true, Name = "season")]
    public string? SeasonFilter { get; set; }
    
    public string? SuccessMessage { get; set; }
    
    public async Task<IActionResult> OnGetAsync()
    {
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
        if (!isAdmin) return RedirectToPage("/Account/AccessDenied");
        
        Packages = await _adminService.GetAllPackagesAsync(includeInactive: true);
        
        FilteredPackages = string.IsNullOrEmpty(SeasonFilter)
            ? Packages
            : Packages.Where(p => p.Season == SeasonFilter.ToLower()).ToList();
        
        SuccessMessage = TempData["SuccessMessage"]?.ToString();
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostToggleStatusAsync(int packageId)
    {
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
        if (!isAdmin) return RedirectToPage("/Account/AccessDenied");
        
        await _adminService.TogglePackageStatusAsync(packageId);
        TempData["SuccessMessage"] = "Package status updated";
        
        return RedirectToPage(new { season = SeasonFilter });
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int packageId)
    {
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
        if (!isAdmin) return RedirectToPage("/Account/AccessDenied");
        
        await _adminService.DeletePackageAsync(packageId);
        TempData["SuccessMessage"] = "Package deleted successfully";
        
        return RedirectToPage(new { season = SeasonFilter });
    }
}
