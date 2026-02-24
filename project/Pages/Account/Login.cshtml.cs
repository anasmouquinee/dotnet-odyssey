using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Services;

namespace project.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;
    
    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }
    
    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ReturnUrl { get; set; }
    
    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        
        public bool RememberMe { get; set; }
    }
    
    public void OnGet(string? returnUrl = null, string? message = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        SuccessMessage = message;
    }
    
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var user = await _authService.LoginAsync(Input.Email, Input.Password);
        
        if (user == null)
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim("FirstName", user.FirstName),
            new Claim("IsAdmin", user.IsAdmin.ToString())
        };
        
        if (user.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }
        
        var identity = new ClaimsIdentity(claims, "CookieAuth");
        var principal = new ClaimsPrincipal(identity);
        
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = Input.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(Input.RememberMe ? 30 : 1)
        };
        
        await HttpContext.SignInAsync("CookieAuth", principal, authProperties);
        
        // Redirect to admin if admin user
        if (user.IsAdmin)
        {
            return LocalRedirect("/Admin");
        }
        
        return LocalRedirect(returnUrl);
    }
}
