using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddPickupAndDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Bookings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsPickupDelivery",
                table: "Bookings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PickupAddress",
                table: "Bookings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsPickupDelivery",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PickupAddress",
                table: "Bookings");
        }
    }
}
