using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;

namespace project.Pages.Destination
{
    public class ViewPageModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public TravelPackage Package { get; set; }
        public string GoogleMapsApiKey { get; set; }

        public ViewPageModel(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Package = await _db.TravelPackages.FirstOrDefaultAsync(p => p.Id == id);

            if (Package == null)
                return NotFound();

            // Get API key from configuration
            GoogleMapsApiKey = _config["GoogleMapsApiKey"] ?? "";

            return Page();
        }
    }
}
