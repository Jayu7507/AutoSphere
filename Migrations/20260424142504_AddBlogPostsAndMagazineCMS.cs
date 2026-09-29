using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutoSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPostsAndMagazineCMS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slug = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Excerpt = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuthorName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReadTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsPublished = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BookingParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    PartId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPriceAtBooking = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingParts_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "BlogPosts",
                columns: new[] { "Id", "AuthorName", "Category", "Content", "CreatedAt", "Excerpt", "ImageUrl", "IsPublished", "ReadTimeMinutes", "Slug", "Title" },
                values: new object[,]
                {
                    { 1, "Mark Veron", "Technology", "Full article content about AI diagnostics...", new DateTime(2026, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Discover how our new digital scanning arrays identify microscopic wear pattern before they become failures.", "https://images.unsplash.com/photo-1486262715619-67b85e0b08d3?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80", true, 5, "ai-revolution-diagnostics", "The AI Revolution in Engine Diagnostic Tools" },
                    { 2, "Sarah Chen", "Maintenance", "Full article content about Ceramic Coating...", new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Protect your investment with 9H Nano-technology. Why standard wax is no longer enough for modern luxury paint.", "https://images.unsplash.com/photo-1599256621730-535171e28e50?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80", true, 3, "ceramic-coating-science", "Ceramic Coating: The Science of Surface Tension" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingParts_BookingId",
                table: "BookingParts",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingParts_PartId",
                table: "BookingParts",
                column: "PartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogPosts");

            migrationBuilder.DropTable(
                name: "BookingParts");
        }
    }
}
