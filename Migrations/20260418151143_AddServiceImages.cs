using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl1",
                table: "Services",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl2",
                table: "Services",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl3",
                table: "Services",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalNotes",
                table: "Bookings",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl1",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ImageUrl2",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ImageUrl3",
                table: "Services");

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "AdditionalNotes",
                keyValue: null,
                column: "AdditionalNotes",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalNotes",
                table: "Bookings",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
