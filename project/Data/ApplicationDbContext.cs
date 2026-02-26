using Microsoft.EntityFrameworkCore;
using project.Models;

namespace project.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<TravelPackage> TravelPackages { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
            
        // TravelPackage configuration
        modelBuilder.Entity<TravelPackage>()
            .Property(t => t.Price)
            .HasPrecision(18, 2);
            
        // Booking configuration
        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalPrice)
            .HasPrecision(18, 2);
            
        // CartItem configuration
        modelBuilder.Entity<CartItem>()
            .HasOne(c => c.User)
            .WithMany(u => u.CartItems)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<CartItem>()
            .HasOne(c => c.TravelPackage)
            .WithMany(t => t.CartItems)
            .HasForeignKey(c => c.TravelPackageId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Booking relationships
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.TravelPackage)
            .WithMany(t => t.Bookings)
            .HasForeignKey(b => b.TravelPackageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
