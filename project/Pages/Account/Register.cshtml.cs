using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Services;

namespace project.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;
    
    public RegisterModel(IAuthService authService)
    {
        _authService = authService;
    }
    
    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    public string? ErrorMessage { get; set; }
    
    public class InputModel
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        public string? PhoneNumber { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    
    public void OnGet()
    {
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var user = await _authService.RegisterAsync(
            Input.FirstName,
            Input.LastName,
            Input.Email,
            Input.Password,
            Input.PhoneNumber
        );
        
        if (user == null)
        {
            ErrorMessage = "An account with this email already exists.";
            return Page();
        }
        
        return RedirectToPage("/Account/Login", new { message = "Account created successfully! Please sign in." });
    }
}
