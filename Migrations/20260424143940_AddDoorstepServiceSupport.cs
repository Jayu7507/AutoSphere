using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddDoorstepServiceSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDoorstepAvailable",
                table: "Services",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDoorstepService",
                table: "Bookings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ServiceAddress",
                table: "Bookings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDoorstepAvailable",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsDoorstepService",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ServiceAddress",
                table: "Bookings");
        }
    }
}
