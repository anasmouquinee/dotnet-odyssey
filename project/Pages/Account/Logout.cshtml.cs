using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace project.Pages.Account;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToPage("/Index");
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToPage("/Index");
    }
}
