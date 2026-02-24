using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Models;
using project.Services;

namespace project.Pages.Admin.Packages;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class CreateModel : PageModel
{
    private readonly IAdminService _adminService;
    
    public CreateModel(IAdminService adminService)
    {
        _adminService = adminService;
    }
    
    [BindProperty]
    public string Destination { get; set; } = string.Empty;
    
    [BindProperty]
    public string Description { get; set; } = string.Empty;
    
    [BindProperty]
    public decimal Price { get; set; }
    
    [BindProperty]
    public string Season { get; set; } = "summer";
    
    [BindProperty]
    public string ImageUrl { get; set; } = string.Empty;
    
    [BindProperty]
    public DateTime DefaultStartDate { get; set; }
    
    [BindProperty]
    public DateTime DefaultEndDate { get; set; }
    
    [BindProperty]
    public int DurationDays { get; set; }
    
    [BindProperty]
    public bool IsActive { get; set; } = true;
    
    public IActionResult OnGet()
    {
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
        if (!isAdmin) return RedirectToPage("/Account/AccessDenied");
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
        if (!isAdmin) return RedirectToPage("/Account/AccessDenied");
        
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var package = new TravelPackage
        {
            Destination = Destination,
            Description = Description,
            Price = Price,
            Season = Season.ToLower(),
            ImageUrl = ImageUrl,
            DefaultStartDate = DefaultStartDate,
            DefaultEndDate = DefaultEndDate,
            DurationDays = DurationDays > 0 ? DurationDays : (int)(DefaultEndDate - DefaultStartDate).TotalDays,
            IsActive = IsActive
        };
        
        await _adminService.CreatePackageAsync(package);
        
        TempData["SuccessMessage"] = $"Package '{Destination}' created successfully!";
        return RedirectToPage("/Admin/Packages");
    }
}
