using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutoSphere.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 100, "Admin" },
                    { 101, "Staff" },
                    { 102, "User" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsEmailVerified", "PasswordHash", "Phone", "RoleId", "UpdatedAt" },
                values: new object[] { 100, new DateTime(2026, 4, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@autosphere.com", "System Administrator", true, "$2a$11$CvhWy4Ay0j5Mo72t6VYPcuYgL55Qwri1SOaberTX1Yn/mTPJzY/gu", "0000000000", 100, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 100);
        }
    }
}
