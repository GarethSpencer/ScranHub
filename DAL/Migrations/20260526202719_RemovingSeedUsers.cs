using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemovingSeedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Active", "Admin", "AuthId", "CreatedBy", "CreatedOn", "DisplayName", "Email", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), true, true, null, new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Admin User", "admin@example.com", null, null },
                    { new Guid("00000000-0000-0000-0000-000000000002"), true, false, null, new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Non-Admin User", "nonadmin@example.com", null, null }
                });
        }
    }
}
