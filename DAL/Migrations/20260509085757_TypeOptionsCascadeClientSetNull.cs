using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class TypeOptionsCascadeClientSetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_FoodTypeOptions_FoodTypeOptionId",
                table: "GroupVenues");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_VenueTypeOptions_VenueTypeOptionId",
                table: "GroupVenues");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_FoodTypeOptions_FoodTypeOptionId",
                table: "GroupVenues",
                column: "FoodTypeOptionId",
                principalTable: "FoodTypeOptions",
                principalColumn: "FoodTypeOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_VenueTypeOptions_VenueTypeOptionId",
                table: "GroupVenues",
                column: "VenueTypeOptionId",
                principalTable: "VenueTypeOptions",
                principalColumn: "VenueTypeOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_FoodTypeOptions_FoodTypeOptionId",
                table: "GroupVenues");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_VenueTypeOptions_VenueTypeOptionId",
                table: "GroupVenues");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_FoodTypeOptions_FoodTypeOptionId",
                table: "GroupVenues",
                column: "FoodTypeOptionId",
                principalTable: "FoodTypeOptions",
                principalColumn: "FoodTypeOptionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_VenueTypeOptions_VenueTypeOptionId",
                table: "GroupVenues",
                column: "VenueTypeOptionId",
                principalTable: "VenueTypeOptions",
                principalColumn: "VenueTypeOptionId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
