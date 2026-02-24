using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project.Services;

namespace project.Pages.Api.Destinations;

public class SearchModel : PageModel
{
    private readonly IDestinationApiService _destinationService;
    
    public SearchModel(IDestinationApiService destinationService)
    {
        _destinationService = destinationService;
    }
    
    public async Task<IActionResult> OnGetAsync(string? query, string? season)
    {
        var suggestions = await _destinationService.SearchDestinationsAsync(query ?? "", season);
        
        return new JsonResult(new { suggestions });
    }
}
