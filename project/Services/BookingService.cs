using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;

namespace project.Services;

public interface IBookingService
{
    Task<List<Booking>> GetUserBookingsAsync(int userId);
    Task<Booking?> GetBookingByIdAsync(int bookingId, int userId);
    Task<Booking?> CreateBookingAsync(int userId, int packageId, DateTime startDate, DateTime endDate, int guests, string? specialRequests);
    Task<List<Booking>> CheckoutCartAsync(int userId);
    Task<bool> CancelBookingAsync(int bookingId, int userId);
    Task<bool> UpdateBookingAsync(int bookingId, int userId, DateTime startDate, DateTime endDate, int guests, string? specialRequests);
}

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;
    private readonly ICartService _cartService;
    
    public BookingService(ApplicationDbContext context, ICartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }
    
    public async Task<List<Booking>> GetUserBookingsAsync(int userId)
    {
        return await _context.Bookings
            .Include(b => b.TravelPackage)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();
    }
    
    public async Task<Booking?> GetBookingByIdAsync(int bookingId, int userId)
    {
        return await _context.Bookings
            .Include(b => b.TravelPackage)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
    }
    
    public async Task<Booking?> CreateBookingAsync(int userId, int packageId, DateTime startDate, DateTime endDate, int guests, string? specialRequests)
    {
        var package = await _context.TravelPackages.FindAsync(packageId);
        if (package == null) return null;
        
        var booking = new Booking
        {
            UserId = userId,
            TravelPackageId = packageId,
            StartDate = startDate,
            EndDate = endDate,
            NumberOfGuests = guests,
            SpecialRequests = specialRequests,
            TotalPrice = package.Price * guests,
            Status = BookingStatus.Pending
        };
        
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        
        return booking;
    }
    
    public async Task<List<Booking>> CheckoutCartAsync(int userId)
    {
        var cartItems = await _context.CartItems
            .Include(c => c.TravelPackage)
            .Where(c => c.UserId == userId)
            .ToListAsync();
            
        if (!cartItems.Any()) return new List<Booking>();
        
        var bookings = new List<Booking>();
        
        foreach (var item in cartItems)
        {
            var booking = new Booking
            {
                UserId = userId,
                TravelPackageId = item.TravelPackageId,
                StartDate = item.SelectedStartDate,
                EndDate = item.SelectedEndDate,
                NumberOfGuests = item.NumberOfGuests,
                SpecialRequests = item.SpecialRequests,
                TotalPrice = item.TravelPackage.Price * item.NumberOfGuests,
                Status = BookingStatus.Pending
            };
            
            bookings.Add(booking);
        }
        
        _context.Bookings.AddRange(bookings);
        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();
        
        return bookings;
    }
    
    public async Task<bool> CancelBookingAsync(int bookingId, int userId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
            
        if (booking == null || booking.Status == BookingStatus.Cancelled) return false;
        
        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> UpdateBookingAsync(int bookingId, int userId, DateTime startDate, DateTime endDate, int guests, string? specialRequests)
    {
        var booking = await _context.Bookings
            .Include(b => b.TravelPackage)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
            
        if (booking == null || booking.Status == BookingStatus.Cancelled) return false;
        
        booking.StartDate = startDate;
        booking.EndDate = endDate;
        booking.NumberOfGuests = guests;
        booking.SpecialRequests = specialRequests;
        booking.TotalPrice = booking.TravelPackage.Price * guests;
        
        return await _context.SaveChangesAsync() > 0;
    }
}
