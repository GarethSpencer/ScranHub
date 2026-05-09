using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class SplitRatingOptionsFromTypeOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "VenueTypeOptions");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "FoodTypeOptions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "VenueTypeOptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "FoodTypeOptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "FoodTypeOptions",
                keyColumn: "FoodTypeOptionId",
                keyValue: new Guid("00000000-0000-0000-0003-000000000001"),
                column: "DisplayOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "FoodTypeOptions",
                keyColumn: "FoodTypeOptionId",
                keyValue: new Guid("00000000-0000-0000-0003-000000000002"),
                column: "DisplayOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "FoodTypeOptions",
                keyColumn: "FoodTypeOptionId",
                keyValue: new Guid("00000000-0000-0000-0003-000000000003"),
                column: "DisplayOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "FoodTypeOptions",
                keyColumn: "FoodTypeOptionId",
                keyValue: new Guid("00000000-0000-0000-0003-000000000004"),
                column: "DisplayOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "FoodTypeOptions",
                keyColumn: "FoodTypeOptionId",
                keyValue: new Guid("00000000-0000-0000-0003-000000000005"),
                column: "DisplayOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "FoodTypeOptions",
                keyColumn: "FoodTypeOptionId",
                keyValue: new Guid("00000000-0000-0000-0003-000000000006"),
                column: "DisplayOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "VenueTypeOptions",
                keyColumn: "VenueTypeOptionId",
                keyValue: new Guid("00000000-0000-0000-0004-000000000001"),
                column: "DisplayOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "VenueTypeOptions",
                keyColumn: "VenueTypeOptionId",
                keyValue: new Guid("00000000-0000-0000-0004-000000000002"),
                column: "DisplayOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "VenueTypeOptions",
                keyColumn: "VenueTypeOptionId",
                keyValue: new Guid("00000000-0000-0000-0004-000000000003"),
                column: "DisplayOrder",
                value: 3);
        }
    }
}
