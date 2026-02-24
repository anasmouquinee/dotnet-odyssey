using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;

namespace project.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(string firstName, string lastName, string email, string password, string? phoneNumber);
    Task<User?> LoginAsync(string email, string password);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> UpdateUserAsync(User user);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    
    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<User?> RegisterAsync(string firstName, string lastName, string email, string password, string? phoneNumber)
    {
        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            return null;
        }
        
        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email.ToLower(),
            PasswordHash = HashPassword(password),
            PhoneNumber = phoneNumber
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return user;
    }
    
    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLower());
            
        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }
        
        return user;
    }
    
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
    }
    
    public async Task<bool> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
