using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Models;
using project.Services;

namespace project.Pages.Admin.Packages;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class EditModel : PageModel
{
    private readonly IAdminService _adminService;
    
    public EditModel(IAdminService adminService)
    {
        _adminService = adminService;
    }
    
    public TravelPackage? Package { get; set; }
    
    [BindProperty]
    public int Id { get; set; }
    
    [BindProperty]
    public string Destination { get; set; } = string.Empty;
    
    [BindProperty]
    public string Description { get; set; } = string.Empty;
    
    [BindProperty]
    public decimal Price { get; set; }
    
    [BindProperty]
    public string Season { get; set; } = string.Empty;
    
    [BindProperty]
    public string ImageUrl { get; set; } = string.Empty;
    
    [BindProperty]
    public DateTime DefaultStartDate { get; set; }
    
    [BindProperty]
    public DateTime DefaultEndDate { get; set; }
    
    [BindProperty]
    public int DurationDays { get; set; }
    
    [BindProperty]
    public bool IsActive { get; set; }
    
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
        if (!isAdmin) return RedirectToPage("/Account/AccessDenied");
        
        Package = await _adminService.GetPackageByIdAsync(id);
        
        if (Package == null)
        {
            return Page();
        }
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
        if (!isAdmin) return RedirectToPage("/Account/AccessDenied");
        
        var package = new TravelPackage
        {
            Id = Id,
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
        
        await _adminService.UpdatePackageAsync(package);
        
        TempData["SuccessMessage"] = $"Package '{Destination}' updated successfully!";
        return RedirectToPage("/Admin/Packages");
    }
}
