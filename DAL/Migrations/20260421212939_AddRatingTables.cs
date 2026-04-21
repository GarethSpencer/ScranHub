using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_CostOptions_CostOptionId",
                table: "GroupVenues");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_RatingOptions_RatingOptionId",
                table: "GroupVenues");

            migrationBuilder.DropIndex(
                name: "IX_GroupVenues_CostOptionId",
                table: "GroupVenues");

            migrationBuilder.DropIndex(
                name: "IX_GroupVenues_RatingOptionId",
                table: "GroupVenues");

            migrationBuilder.DropColumn(
                name: "CostOptionId",
                table: "GroupVenues");

            migrationBuilder.DropColumn(
                name: "RatingOptionId",
                table: "GroupVenues");

            migrationBuilder.CreateTable(
                name: "CostUserRatings",
                columns: table => new
                {
                    CostUserRatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupVenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CostOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostUserRatings", x => x.CostUserRatingId);
                    table.ForeignKey(
                        name: "FK_CostUserRatings_CostOptions_CostOptionId",
                        column: x => x.CostOptionId,
                        principalTable: "CostOptions",
                        principalColumn: "CostOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CostUserRatings_GroupVenues_GroupVenueId",
                        column: x => x.GroupVenueId,
                        principalTable: "GroupVenues",
                        principalColumn: "GroupVenueId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CostUserRatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RatingUserRatings",
                columns: table => new
                {
                    RatingUserRatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupVenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RatingOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingUserRatings", x => x.RatingUserRatingId);
                    table.ForeignKey(
                        name: "FK_RatingUserRatings_GroupVenues_GroupVenueId",
                        column: x => x.GroupVenueId,
                        principalTable: "GroupVenues",
                        principalColumn: "GroupVenueId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RatingUserRatings_RatingOptions_RatingOptionId",
                        column: x => x.RatingOptionId,
                        principalTable: "RatingOptions",
                        principalColumn: "RatingOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RatingUserRatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostUserRatings_CostOptionId",
                table: "CostUserRatings",
                column: "CostOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CostUserRatings_GroupVenueId",
                table: "CostUserRatings",
                column: "GroupVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_CostUserRatings_UserId",
                table: "CostUserRatings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RatingUserRatings_GroupVenueId",
                table: "RatingUserRatings",
                column: "GroupVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_RatingUserRatings_RatingOptionId",
                table: "RatingUserRatings",
                column: "RatingOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_RatingUserRatings_UserId",
                table: "RatingUserRatings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostUserRatings");

            migrationBuilder.DropTable(
                name: "RatingUserRatings");

            migrationBuilder.AddColumn<Guid>(
                name: "CostOptionId",
                table: "GroupVenues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RatingOptionId",
                table: "GroupVenues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupVenues_CostOptionId",
                table: "GroupVenues",
                column: "CostOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupVenues_RatingOptionId",
                table: "GroupVenues",
                column: "RatingOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_CostOptions_CostOptionId",
                table: "GroupVenues",
                column: "CostOptionId",
                principalTable: "CostOptions",
                principalColumn: "CostOptionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_RatingOptions_RatingOptionId",
                table: "GroupVenues",
                column: "RatingOptionId",
                principalTable: "RatingOptions",
                principalColumn: "RatingOptionId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
