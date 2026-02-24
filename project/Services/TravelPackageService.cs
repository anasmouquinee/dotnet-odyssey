using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;

namespace project.Services;

public interface ITravelPackageService
{
    Task<List<TravelPackage>> GetAllPackagesAsync();
    Task<List<TravelPackage>> GetPackagesBySeasonAsync(string season);
    Task<TravelPackage?> GetPackageByIdAsync(int id);
}

public class TravelPackageService : ITravelPackageService
{
    private readonly ApplicationDbContext _context;
    
    public TravelPackageService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<TravelPackage>> GetAllPackagesAsync()
    {
        return await _context.TravelPackages
            .Where(p => p.IsActive)
            .OrderBy(p => p.Season)
            .ThenBy(p => p.Destination)
            .ToListAsync();
    }
    
    public async Task<List<TravelPackage>> GetPackagesBySeasonAsync(string season)
    {
        return await _context.TravelPackages
            .Where(p => p.IsActive && p.Season == season.ToLower())
            .OrderBy(p => p.Destination)
            .ToListAsync();
    }
    
    public async Task<TravelPackage?> GetPackageByIdAsync(int id)
    {
        return await _context.TravelPackages.FindAsync(id);
    }
}
