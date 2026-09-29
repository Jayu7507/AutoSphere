using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSphere.Migrations
{
    /// <inheritdoc />
    public partial class EnableDoorstepForExistingServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Services SET IsDoorstepAvailable = 1 WHERE Name LIKE '%Oil Change%' OR Name LIKE '%Inspection%' OR Name LIKE '%Wash%'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
