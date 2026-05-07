using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameCostRatingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostUserRatings");

            migrationBuilder.CreateTable(
                name: "CostRatings",
                columns: table => new
                {
                    CostRatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_CostRatings", x => x.CostRatingId);
                    table.ForeignKey(
                        name: "FK_CostRatings_CostOptions_CostOptionId",
                        column: x => x.CostOptionId,
                        principalTable: "CostOptions",
                        principalColumn: "CostOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CostRatings_GroupVenues_GroupVenueId",
                        column: x => x.GroupVenueId,
                        principalTable: "GroupVenues",
                        principalColumn: "GroupVenueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CostRatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostRatings_CostOptionId",
                table: "CostRatings",
                column: "CostOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CostRatings_GroupVenueId",
                table: "CostRatings",
                column: "GroupVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_CostRatings_UserId",
                table: "CostRatings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostRatings");

            migrationBuilder.CreateTable(
                name: "CostUserRatings",
                columns: table => new
                {
                    CostUserRatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CostOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupVenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CostUserRatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
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
        }
    }
}
