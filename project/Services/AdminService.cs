using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;

namespace project.Services;

public interface IAdminService
{
    // Package Management
    Task<List<TravelPackage>> GetAllPackagesAsync(bool includeInactive = true);
    Task<TravelPackage?> GetPackageByIdAsync(int id);
    Task<TravelPackage> CreatePackageAsync(TravelPackage package);
    Task<TravelPackage?> UpdatePackageAsync(TravelPackage package);
    Task<bool> DeletePackageAsync(int id);
    Task<bool> TogglePackageStatusAsync(int id);
    
    // Order/Booking Management
    Task<List<Booking>> GetAllBookingsAsync();
    Task<List<Booking>> GetBookingsByStatusAsync(BookingStatus status);
    Task<Booking?> GetBookingByIdAsync(int id);
    Task<bool> UpdateBookingStatusAsync(int bookingId, BookingStatus status);
    
    // Statistics
    Task<AdminDashboardStats> GetDashboardStatsAsync();
    
    // User Management
    Task<List<User>> GetAllUsersAsync();
    Task<bool> ToggleAdminStatusAsync(int userId);
}

public class AdminDashboardStats
{
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int TotalPackages { get; set; }
    public int ActivePackages { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public Dictionary<string, int> BookingsBySeason { get; set; } = new();
    public List<Booking> RecentBookings { get; set; } = new();
}

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    
    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Package Management
    public async Task<List<TravelPackage>> GetAllPackagesAsync(bool includeInactive = true)
    {
        var query = _context.TravelPackages.AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }
        
        return await query
            .OrderBy(p => p.Season)
            .ThenBy(p => p.Destination)
            .ToListAsync();
    }
    
    public async Task<TravelPackage?> GetPackageByIdAsync(int id)
    {
        return await _context.TravelPackages.FindAsync(id);
    }
    
    public async Task<TravelPackage> CreatePackageAsync(TravelPackage package)
    {
        _context.TravelPackages.Add(package);
        await _context.SaveChangesAsync();
        return package;
    }
    
    public async Task<TravelPackage?> UpdatePackageAsync(TravelPackage package)
    {
        var existing = await _context.TravelPackages.FindAsync(package.Id);
        if (existing == null) return null;
        
        existing.Destination = package.Destination;
        existing.Description = package.Description;
        existing.Price = package.Price;
        existing.Season = package.Season;
        existing.ImageUrl = package.ImageUrl;
        existing.DefaultStartDate = package.DefaultStartDate;
        existing.DefaultEndDate = package.DefaultEndDate;
        existing.DurationDays = package.DurationDays;
        existing.IsActive = package.IsActive;
        
        await _context.SaveChangesAsync();
        return existing;
    }
    
    public async Task<bool> DeletePackageAsync(int id)
    {
        var package = await _context.TravelPackages.FindAsync(id);
        if (package == null) return false;
        
        _context.TravelPackages.Remove(package);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> TogglePackageStatusAsync(int id)
    {
        var package = await _context.TravelPackages.FindAsync(id);
        if (package == null) return false;
        
        package.IsActive = !package.IsActive;
        return await _context.SaveChangesAsync() > 0;
    }
    
    // Order/Booking Management
    public async Task<List<Booking>> GetAllBookingsAsync()
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.TravelPackage)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();
    }
    
    public async Task<List<Booking>> GetBookingsByStatusAsync(BookingStatus status)
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.TravelPackage)
            .Where(b => b.Status == status)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();
    }
    
    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.TravelPackage)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
    
    public async Task<bool> UpdateBookingStatusAsync(int bookingId, BookingStatus status)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null) return false;
        
        booking.Status = status;
        
        if (status == BookingStatus.Confirmed)
        {
            booking.ConfirmedAt = DateTime.UtcNow;
        }
        else if (status == BookingStatus.Cancelled)
        {
            booking.CancelledAt = DateTime.UtcNow;
        }
        
        return await _context.SaveChangesAsync() > 0;
    }
    
    // Statistics
    public async Task<AdminDashboardStats> GetDashboardStatsAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        
        var allBookings = await _context.Bookings
            .Include(b => b.TravelPackage)
            .ToListAsync();
        
        var stats = new AdminDashboardStats
        {
            TotalBookings = allBookings.Count,
            PendingBookings = allBookings.Count(b => b.Status == BookingStatus.Pending),
            ConfirmedBookings = allBookings.Count(b => b.Status == BookingStatus.Confirmed),
            CancelledBookings = allBookings.Count(b => b.Status == BookingStatus.Cancelled),
            TotalPackages = await _context.TravelPackages.CountAsync(),
            ActivePackages = await _context.TravelPackages.CountAsync(p => p.IsActive),
            TotalUsers = await _context.Users.CountAsync(),
            TotalRevenue = allBookings
                .Where(b => b.Status != BookingStatus.Cancelled)
                .Sum(b => b.TotalPrice),
            MonthlyRevenue = allBookings
                .Where(b => b.Status != BookingStatus.Cancelled && b.BookedAt >= startOfMonth)
                .Sum(b => b.TotalPrice),
            BookingsBySeason = allBookings
                .Where(b => b.TravelPackage != null)
                .GroupBy(b => b.TravelPackage.Season)
                .ToDictionary(g => g.Key, g => g.Count()),
            RecentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.TravelPackage)
                .OrderByDescending(b => b.BookedAt)
                .Take(10)
                .ToListAsync()
        };
        
        return stats;
    }
    
    // User Management
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<bool> ToggleAdminStatusAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        
        user.IsAdmin = !user.IsAdmin;
        return await _context.SaveChangesAsync() > 0;
    }
}
