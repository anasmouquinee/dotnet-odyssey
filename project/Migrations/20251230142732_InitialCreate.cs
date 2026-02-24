using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace project.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TravelPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Destination = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Season = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DefaultEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TravelPackageId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    SpecialRequests = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BookedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_TravelPackages_TravelPackageId",
                        column: x => x.TravelPackageId,
                        principalTable: "TravelPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TravelPackageId = table.Column<int>(type: "int", nullable: false),
                    SelectedStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SelectedEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    SpecialRequests = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_TravelPackages_TravelPackageId",
                        column: x => x.TravelPackageId,
                        principalTable: "TravelPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TravelPackages",
                columns: new[] { "Id", "DefaultEndDate", "DefaultStartDate", "Description", "Destination", "DurationDays", "ImageUrl", "IsActive", "Price", "Season" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Philosopher's Path Walk", "Kyoto, Japan", 14, "https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e?q=80&w=2070&auto=format&fit=crop", true, 4200m, "spring" },
                    { 2, new DateTime(2025, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Private Tulip Fields Tour", "Keukenhof, Holland", 15, "https://images.unsplash.com/photo-1460500063983-994d4c27756c?q=80&w=2070&auto=format&fit=crop", true, 3100m, "spring" },
                    { 3, new DateTime(2025, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Austral Spring Trek", "Patagonia, Chile", 15, "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?q=80&w=2070&auto=format&fit=crop", true, 5800m, "spring" },
                    { 4, new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Private Yacht Charter", "Amalfi Coast, Italy", 15, "https://images.unsplash.com/photo-1516483638261-f4dbaf036963?q=80&w=2560&auto=format&fit=crop", true, 6500m, "summer" },
                    { 5, new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Caldera Sunset Villas", "Santorini, Greece", 15, "https://images.unsplash.com/photo-1601581875309-fafbf2d3ed2a?q=80&w=2574&auto=format&fit=crop", true, 5200m, "summer" },
                    { 6, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Overwater Sanctuary", "Baa Atoll, Maldives", 10, "https://images.unsplash.com/photo-1540206351-d6465b3ac5c1?q=80&w=2664&auto=format&fit=crop", true, 8900m, "summer" },
                    { 7, new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "New England Foliage Tour", "Vermont, USA", 15, "https://images.unsplash.com/photo-1509565840034-3c385bbe6451?q=80&w=2000&auto=format&fit=crop", true, 3800m, "autumn" },
                    { 8, new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Castle & Forest Route", "Bavaria, Germany", 14, "https://images.unsplash.com/photo-1505307469735-373801f95c47?q=80&w=2670&auto=format&fit=crop", true, 4500m, "autumn" },
                    { 9, new DateTime(2025, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Momiji Maple Viewing", "Arashiyama, Japan", 15, "https://images.unsplash.com/photo-1476611317561-60e1b778edfb?q=80&w=2670&auto=format&fit=crop", true, 4800m, "autumn" },
                    { 10, new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Aurora Glass Igloos", "Lapland, Finland", 26, "https://images.unsplash.com/photo-1518182170546-0766ce6fec56?q=80&w=2000&auto=format&fit=crop", true, 5500m, "winter" },
                    { 11, new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Matterhorn Ski Chalet", "Zermatt, Switzerland", 26, "https://images.unsplash.com/photo-1551524357-1249fa6b2eb1?q=80&w=2670&auto=format&fit=crop", true, 7200m, "winter" },
                    { 12, new DateTime(2026, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Luxury Winter Retreat", "Aspen, USA", 13, "https://images.unsplash.com/photo-1518096366620-33062d35508a?q=80&w=2670&auto=format&fit=crop", true, 9500m, "winter" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TravelPackageId",
                table: "Bookings",
                column: "TravelPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_TravelPackageId",
                table: "CartItems",
                column: "TravelPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId",
                table: "CartItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "TravelPackages");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
