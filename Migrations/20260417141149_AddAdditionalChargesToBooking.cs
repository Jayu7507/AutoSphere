using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalChargesToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalCost",
                table: "StaffAssignments",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalCharges",
                table: "Bookings",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalCost",
                table: "StaffAssignments");

            migrationBuilder.DropColumn(
                name: "AdditionalCharges",
                table: "Bookings");
        }
    }
}
