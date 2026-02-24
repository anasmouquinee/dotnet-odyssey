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
            
        // Seed travel packages
        SeedTravelPackages(modelBuilder);
    }
    
    private void SeedTravelPackages(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TravelPackage>().HasData(
            // Spring
            new TravelPackage
            {
                Id = 1,
                Destination = "Kyoto, Japan",
                Description = "Philosopher's Path Walk",
                Price = 4200,
                Season = "spring",
                ImageUrl = "https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e?q=80&w=2070&auto=format&fit=crop",
                DefaultStartDate = new DateTime(2025, 4, 1),
                DefaultEndDate = new DateTime(2025, 4, 15),
                DurationDays = 14
            },
            new TravelPackage
            {
                Id = 2,
                Destination = "Keukenhof, Holland",
                Description = "Private Tulip Fields Tour",
                Price = 3100,
                Season = "spring",
                ImageUrl = "https://images.unsplash.com/photo-1460500063983-994d4c27756c?q=80&w=2070&auto=format&fit=crop",
                DefaultStartDate = new DateTime(2025, 4, 20),
                DefaultEndDate = new DateTime(2025, 5, 5),
                DurationDays = 15
            },
            new TravelPackage
            {
                Id = 3,
                Destination = "Patagonia, Chile",
                Description = "Austral Spring Trek",
                Price = 5800,
                Season = "spring",
                ImageUrl = "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?q=80&w=2070&auto=format&fit=crop",
                DefaultStartDate = new DateTime(2025, 10, 10),
                DefaultEndDate = new DateTime(2025, 10, 25),
                DurationDays = 15
            },
            // Summer
            new TravelPackage
            {
                Id = 4,
                Destination = "Amalfi Coast, Italy",
                Description = "Private Yacht Charter",
                Price = 6500,
                Season = "summer",
                ImageUrl = "https://images.unsplash.com/photo-1516483638261-f4dbaf036963?q=80&w=2560&auto=format&fit=crop",
                DefaultStartDate = new DateTime(2025, 6, 15),
                DefaultEndDate = new DateTime(2025, 6, 30),
                DurationDays = 15
            },
            new TravelPackage
            {
                Id = 5,
                Destination = "Santorini, Greece",
                Description = "Caldera Sunset Villas",
                Price = 5200,
                Season = "summer",
                ImageUrl = "https://images.unsplash.com/photo-1601581875309-fafbf2d3ed2a?q=80&w=2574&auto=format&fit=crop",
                DefaultStartDate = new DateTime(2025, 7, 10),
                DefaultEndDate = new DateTime(2025, 7, 25),
                DurationDays = 15
            },
            new TravelPackage
            {
                Id = 6,
                Destination = "Baa Atoll, Maldives",
                Description = "Overwater Sanctuary",
                Price = 8900,
                Season = "summer",
                ImageUrl = "https://images.unsplash.com/photo-1540206351-d6465b3ac5c1?q=80&w=2664&auto=format&fit=crop",
                DefaultStartDate = new DateTime(2025, 8, 5),
                DefaultEndDate = new DateTime(2025, 8, 15),
                DurationDays = 10
            },
            // Autumn
            new TravelPackage
            {
                Id = 7,
                Destination = "Vermont, USA",
                Description = "New England Foliage Tour",
                Price = 3800,
                Season = "autumn",
                ImageUrl = "https://images.unsplash.com/photo-1509565840034-3c385bbe6451?q=80&w=2000&auto=format&fit=crop",
                DefaultStartDate = new DateTime(2025, 9, 25),
                DefaultEndDate = new DateTime(2025, 10, 10),
                DurationDays = 15
            },
            new TravelPackage
            {
                Id = 8,
                Destination = "Bavaria, Germany",
                Description = "Castle & Forest Route",
                Price = 4500,
                Season = "autumn",
                ImageUrl = "https://images.unsplash.com/photo-1505307469735-373801f95c47?q=80&w=2670&auto=format&fit=crop",
                DefaultStartDate = new DateTime(2025, 10, 1),
                DefaultEndDate = new DateTime(2025, 10, 15),
                DurationDays = 14
            },
            new TravelPackage
            {
                Id = 9,
                Destination = "Arashiyama, Japan",
                Description = "Momiji Maple Viewing",
                Price = 4800,
                Season = "autumn",
                ImageUrl = "https://images.unsplash.com/photo-1476611317561-60e1b778edfb?q=80&w=2670&auto=format&fit=crop",
                DefaultStartDate = new DateTime(2025, 11, 15),
                DefaultEndDate = new DateTime(2025, 11, 30),
                DurationDays = 15
            },
            // Winter
            new TravelPackage
            {
                Id = 10,
                Destination = "Lapland, Finland",
                Description = "Aurora Glass Igloos",
                Price = 5500,
                Season = "winter",
                ImageUrl = "https://images.unsplash.com/photo-1518182170546-0766ce6fec56?q=80&w=2000&auto=format&fit=crop",
                DefaultStartDate = new DateTime(2025, 12, 10),
                DefaultEndDate = new DateTime(2026, 1, 5),
                DurationDays = 26
            },
            new TravelPackage
            {
                Id = 11,
                Destination = "Zermatt, Switzerland",
                Description = "Matterhorn Ski Chalet",
                Price = 7200,
                Season = "winter",
                ImageUrl = "https://images.unsplash.com/photo-1551524357-1249fa6b2eb1?q=80&w=2670&auto=format&fit=crop",
                DefaultStartDate = new DateTime(2026, 1, 15),
                DefaultEndDate = new DateTime(2026, 2, 10),
                DurationDays = 26
            },
            new TravelPackage
            {
                Id = 12,
                Destination = "Aspen, USA",
                Description = "Luxury Winter Retreat",
                Price = 9500,
                Season = "winter",
                ImageUrl = "https://images.unsplash.com/photo-1518096366620-33062d35508a?q=80&w=2670&auto=format&fit=crop",
                DefaultStartDate = new DateTime(2026, 2, 20),
                DefaultEndDate = new DateTime(2026, 3, 5),
                DurationDays = 13
            }
        );
    }
}
