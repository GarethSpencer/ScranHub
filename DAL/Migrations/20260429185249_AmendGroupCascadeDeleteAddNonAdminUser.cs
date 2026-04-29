using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AmendGroupCascadeDeleteAddNonAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CostOptions_Groups_GroupId",
                table: "CostOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_CostUserRatings_GroupVenues_GroupVenueId",
                table: "CostUserRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodTypeOptions_Groups_GroupId",
                table: "FoodTypeOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_Groups_GroupId",
                table: "GroupVenues");

            migrationBuilder.DropForeignKey(
                name: "FK_RatingOptions_Groups_GroupId",
                table: "RatingOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_RatingUserRatings_GroupVenues_GroupVenueId",
                table: "RatingUserRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGroups_Groups_GroupId",
                table: "UserGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_VenueTypeOptions_Groups_GroupId",
                table: "VenueTypeOptions");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Active", "Admin", "AuthId", "CreatedBy", "CreatedOn", "DisplayName", "Email", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000002"), true, false, null, new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Non-Admin User", "nonadmin@example.com", null, null });

            migrationBuilder.AddForeignKey(
                name: "FK_CostOptions_Groups_GroupId",
                table: "CostOptions",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CostUserRatings_GroupVenues_GroupVenueId",
                table: "CostUserRatings",
                column: "GroupVenueId",
                principalTable: "GroupVenues",
                principalColumn: "GroupVenueId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodTypeOptions_Groups_GroupId",
                table: "FoodTypeOptions",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_Groups_GroupId",
                table: "GroupVenues",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RatingOptions_Groups_GroupId",
                table: "RatingOptions",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RatingUserRatings_GroupVenues_GroupVenueId",
                table: "RatingUserRatings",
                column: "GroupVenueId",
                principalTable: "GroupVenues",
                principalColumn: "GroupVenueId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroups_Groups_GroupId",
                table: "UserGroups",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VenueTypeOptions_Groups_GroupId",
                table: "VenueTypeOptions",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CostOptions_Groups_GroupId",
                table: "CostOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_CostUserRatings_GroupVenues_GroupVenueId",
                table: "CostUserRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodTypeOptions_Groups_GroupId",
                table: "FoodTypeOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_Groups_GroupId",
                table: "GroupVenues");

            migrationBuilder.DropForeignKey(
                name: "FK_RatingOptions_Groups_GroupId",
                table: "RatingOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_RatingUserRatings_GroupVenues_GroupVenueId",
                table: "RatingUserRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGroups_Groups_GroupId",
                table: "UserGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_VenueTypeOptions_Groups_GroupId",
                table: "VenueTypeOptions");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.AddForeignKey(
                name: "FK_CostOptions_Groups_GroupId",
                table: "CostOptions",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CostUserRatings_GroupVenues_GroupVenueId",
                table: "CostUserRatings",
                column: "GroupVenueId",
                principalTable: "GroupVenues",
                principalColumn: "GroupVenueId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodTypeOptions_Groups_GroupId",
                table: "FoodTypeOptions",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_Groups_GroupId",
                table: "GroupVenues",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RatingOptions_Groups_GroupId",
                table: "RatingOptions",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RatingUserRatings_GroupVenues_GroupVenueId",
                table: "RatingUserRatings",
                column: "GroupVenueId",
                principalTable: "GroupVenues",
                principalColumn: "GroupVenueId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroups_Groups_GroupId",
                table: "UserGroups",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VenueTypeOptions_Groups_GroupId",
                table: "VenueTypeOptions",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
