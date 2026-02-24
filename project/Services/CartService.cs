using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;

namespace project.Services;

public interface ICartService
{
    Task<List<CartItem>> GetCartItemsAsync(int userId);
    Task<CartItem?> AddToCartAsync(int userId, int packageId, DateTime startDate, DateTime endDate, int guests, string? specialRequests);
    Task<CartItem?> UpdateCartItemAsync(int cartItemId, int userId, DateTime startDate, DateTime endDate, int guests, string? specialRequests);
    Task<bool> RemoveFromCartAsync(int cartItemId, int userId);
    Task<bool> ClearCartAsync(int userId);
    Task<int> GetCartCountAsync(int userId);
    Task<decimal> GetCartTotalAsync(int userId);
}

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;
    
    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<CartItem>> GetCartItemsAsync(int userId)
    {
        return await _context.CartItems
            .Include(c => c.TravelPackage)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.AddedAt)
            .ToListAsync();
    }
    
    public async Task<CartItem?> AddToCartAsync(int userId, int packageId, DateTime startDate, DateTime endDate, int guests, string? specialRequests)
    {
        var package = await _context.TravelPackages.FindAsync(packageId);
        if (package == null) return null;
        
        // Check if item already in cart
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.TravelPackageId == packageId);
            
        if (existingItem != null)
        {
            // Update existing item
            existingItem.SelectedStartDate = startDate;
            existingItem.SelectedEndDate = endDate;
            existingItem.NumberOfGuests = guests;
            existingItem.SpecialRequests = specialRequests;
            await _context.SaveChangesAsync();
            return existingItem;
        }
        
        var cartItem = new CartItem
        {
            UserId = userId,
            TravelPackageId = packageId,
            SelectedStartDate = startDate,
            SelectedEndDate = endDate,
            NumberOfGuests = guests,
            SpecialRequests = specialRequests
        };
        
        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();
        
        return cartItem;
    }
    
    public async Task<CartItem?> UpdateCartItemAsync(int cartItemId, int userId, DateTime startDate, DateTime endDate, int guests, string? specialRequests)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
            
        if (cartItem == null) return null;
        
        cartItem.SelectedStartDate = startDate;
        cartItem.SelectedEndDate = endDate;
        cartItem.NumberOfGuests = guests;
        cartItem.SpecialRequests = specialRequests;
        
        await _context.SaveChangesAsync();
        return cartItem;
    }
    
    public async Task<bool> RemoveFromCartAsync(int cartItemId, int userId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
            
        if (cartItem == null) return false;
        
        _context.CartItems.Remove(cartItem);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> ClearCartAsync(int userId)
    {
        var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
        _context.CartItems.RemoveRange(items);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<int> GetCartCountAsync(int userId)
    {
        return await _context.CartItems.CountAsync(c => c.UserId == userId);
    }
    
    public async Task<decimal> GetCartTotalAsync(int userId)
    {
        return await _context.CartItems
            .Include(c => c.TravelPackage)
            .Where(c => c.UserId == userId)
            .SumAsync(c => c.TravelPackage.Price * c.NumberOfGuests);
    }
}
