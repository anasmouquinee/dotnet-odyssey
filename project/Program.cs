using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using project.Data;
using project.Models;
using project.Services;

namespace project
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            
            // Add DbContext - uses PostgreSQL in production, SQL Server locally
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (connectionString!.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
                connectionString = ConvertPostgresUrlToNpgsql(connectionString);

            if (builder.Environment.IsProduction() || connectionString.StartsWith("Host="))
            {
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(connectionString));
            }
            else
            {
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString,
                        sqlOptions => sqlOptions.EnableRetryOnFailure()));
            }
            
            // Add Authentication
            builder.Services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                });
            
            // Add custom services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<ITravelPackageService, TravelPackageService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            
            // Add HttpClient for external API calls
            builder.Services.AddHttpClient<IDestinationApiService, DestinationApiService>();

            var app = builder.Build();
            
            // Ensure database is created and seeded
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                
                try
                {
                    if (context.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
                    {
                        // Migrations were generated for SQL Server; apply the schema
                        // directly from the current model when running on PostgreSQL.
                        context.Database.EnsureCreated();
                    }
                    else
                    {
                        context.Database.Migrate();
                    }

                    await SeedTravelPackagesAsync(context, logger);
                    await SeedAdminUserAsync(context, logger);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            // Minimal API endpoint for destination search
            app.MapGet("/api/destinations/search", async (string? query, string? season, IDestinationApiService destinationService) =>
            {
                var suggestions = await destinationService.SearchDestinationsAsync(query ?? "", season);
                return Results.Json(new { suggestions });
            });

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
        
        private static async Task SeedAdminUserAsync(ApplicationDbContext context, ILogger logger)
        {
            const string adminEmail = "anasmouquine@gmail.com";
            const string adminPassword = "anaskaelar2004";
            const string adminFirstName = "Anas";
            const string adminLastName = "Admin";
            
            // Check if admin already exists
            var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            
            if (existingAdmin == null)
            {
                // Create admin user
                var adminUser = new User
                {
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    Email = adminEmail,
                    PasswordHash = HashPassword(adminPassword),
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
                
                logger.LogInformation("Admin user created successfully: {Email}", adminEmail);
            }
            else if (!existingAdmin.IsAdmin)
            {
                // Ensure user is admin
                existingAdmin.IsAdmin = true;
                await context.SaveChangesAsync();
                
                logger.LogInformation("Existing user promoted to admin: {Email}", adminEmail);
            }
            else
            {
                logger.LogInformation("Admin user already exists: {Email}", adminEmail);
            }
        }
        
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static async Task SeedTravelPackagesAsync(ApplicationDbContext context, ILogger logger)
        {
            if (await context.TravelPackages.AnyAsync()) return;

            var packages = new[]
            {
                new project.Models.TravelPackage { Destination = "Kyoto, Japan", Description = "Philosopher's Path Walk", Price = 4200, Season = "spring", ImageUrl = "https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e?q=80&w=2070", DefaultStartDate = new DateTime(2025,4,1,0,0,0,DateTimeKind.Utc), DefaultEndDate = new DateTime(2025,4,15,0,0,0,DateTimeKind.Utc), DurationDays = 14, IsActive = true },
                new project.Models.TravelPackage { Destination = "Keukenhof, Holland", Description = "Private Tulip Fields Tour", Price = 3100, Season = "spring", ImageUrl = "https://images.unsplash.com/photo-1460500063983-994d4c27756c?q=80&w=2070", DefaultStartDate = new DateTime(2025,4,20,0,0,0,DateTimeKind.Utc), DefaultEndDate = new DateTime(2025,5,5,0,0,0,DateTimeKind.Utc), DurationDays = 15, IsActive = true },
                new project.Models.TravelPackage { Destination = "Patagonia, Chile", Description = "Austral Spring Trek", Price = 5800, Season = "spring", ImageUrl = "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?q=80&w=2070", DefaultStartDate = new DateTime(2025,10,10,0,0,0,DateTimeKind.Utc), DefaultEndDate = new DateTime(2025,10,25,0,0,0,DateTimeKind.Utc), DurationDays = 15, IsActive = true },
                new project.Models.TravelPackage { Destination = "Amalfi Coast, Italy", Description = "Private Yacht Charter", Price = 6500, Season = "summer", ImageUrl = "https://images.unsplash.com/photo-1516483638261-f4dbaf036963?q=80&w=2560", DefaultStartDate = new DateTime(2025,6,15,0,0,0,DateTimeKind.Utc), DefaultEndDate = new DateTime(2025,6,30,0,0,0,DateTimeKind.Utc), DurationDays = 15, IsActive = true },
                new project.Models.TravelPackage { Destination = "Santorini, Greece", Description = "Caldera Sunset Villas", Price = 5200, Season = "summer", ImageUrl = "https://images.unsplash.com/photo-1601581875309-fafbf2d3ed2a?q=80&w=2574", DefaultStartDate = new DateTime(2025,7,10,0,0,0,DateTimeKind.Utc), DefaultEndDate = new DateTime(2025,7,25,0,0,0,DateTimeKind.Utc), DurationDays = 15, IsActive = true },
                new project.Models.TravelPackage { Destination = "Baa Atoll, Maldives", Description = "Overwater Sanctuary", Price = 8900, Season = "summer", ImageUrl = "https://images.unsplash.com/photo-1540206351-d6465b3ac5c1?q=80&w=2664", DefaultStartDate = new DateTime(2025,8,5,0,0,0,DateTimeKind.Utc), DefaultEndDate = new DateTime(2025,8,15,0,0,0,DateTimeKind.Utc), DurationDays = 10, IsActive = true },
                new project.Models.TravelPackage { Destination = "Vermont, USA", Description = "New England Foliage Tour", Price = 3800, Season = "autumn", ImageUrl = "https://images.unsplash.com/photo-1509565840034-3c385bbe6451?q=80&w=2000", DefaultStartDate = new DateTime(2025,9,25,0,0,0,DateTimeKind.Utc), DefaultEndDate = new DateTime(2025,10,10,0,0,0,DateTimeKind.Utc), DurationDays = 15, IsActive = true },
                new project.Models.TravelPackage { Destination = "Bavaria, Germany", Description = "Castle & Forest Route", Price = 4500, Season = "autumn", ImageUrl = "https://images.unsplash.com/photo-1505307469735-373801f95c47?q=80&w=2670", DefaultStartDate = new DateTime(2025,10,1,0,0,0,DateTimeKind.Utc), DefaultEndDate = new DateTime(2025,10,15,0,0,0,DateTimeKind.Utc), DurationDays = 14, IsActive = true },
                new project.Models.TravelPackage { Destination = "Arashiyama, Japan", Description = "Momiji Maple Viewing", Price = 4800, Season = "autumn", ImageUrl = "https://images.unsplash.com/photo-1476611317561-60e1b778edfb?q=80&w=2670", DefaultStartDate = new DateTime(2025,11,15,0,0,0,DateTimeKind.Utc), DefaultEndDate = new DateTime(2025,11,30,0,0,0,DateTimeKind.Utc), DurationDays = 15, IsActive = true },
                new project.Models.TravelPackage { Destination = "Lapland, Finland", Description = "Aurora Glass Igloos", Price = 5500, Season = "winter", ImageUrl = "https://images.unsplash.com/photo-1518182170546-0766ce6fec56?q=80&w=2000", DefaultStartDate = new DateTime(2025,12,10,0,0,0,DateTimeKind.Utc), DefaultEndDate = new DateTime(2026,1,5,0,0,0,DateTimeKind.Utc), DurationDays = 26, IsActive = true },
                new project.Models.TravelPackage { Destination = "Zermatt, Switzerland", Description = "Matterhorn Ski Chalet", Price = 7200, Season = "winter", ImageUrl = "https://images.unsplash.com/photo-1551524357-1249fa6b2eb1?q=80&w=2670", DefaultStartDate = new DateTime(2026,1,15,0,0,0,DateTimeKind.Utc), DefaultEndDate = new DateTime(2026,2,10,0,0,0,DateTimeKind.Utc), DurationDays = 26, IsActive = true },
                new project.Models.TravelPackage { Destination = "Aspen, USA", Description = "Luxury Winter Retreat", Price = 9500, Season = "winter", ImageUrl = "https://images.unsplash.com/photo-1518096366620-33062d35508a?q=80&w=2670", DefaultStartDate = new DateTime(2026,2,20,0,0,0,DateTimeKind.Utc), DefaultEndDate = new DateTime(2026,3,5,0,0,0,DateTimeKind.Utc), DurationDays = 13, IsActive = true },
            };

            await context.TravelPackages.AddRangeAsync(packages);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} travel packages.", packages.Length);
        }

        private static string ConvertPostgresUrlToNpgsql(string url)
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':', 2);
            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = uri.AbsolutePath.TrimStart('/'),
                Username = Uri.UnescapeDataString(userInfo[0]),
                Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
                SslMode = SslMode.Require
            };
            return csb.ConnectionString;
        }
    }
}
