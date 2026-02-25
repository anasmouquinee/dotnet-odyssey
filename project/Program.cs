using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
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
                    context.Database.Migrate(); // This will create the database and apply migrations
                    
                    // Seed admin user if not exists
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

        private static string ConvertPostgresUrlToNpgsql(string url)
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':', 2);
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
    }
}
