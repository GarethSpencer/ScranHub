using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AmendCostAndRatingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupCostOverrides");

            migrationBuilder.DropTable(
                name: "GroupCosts");

            migrationBuilder.DropTable(
                name: "GroupRatingOverrides");

            migrationBuilder.DropTable(
                name: "GroupRatings");

            migrationBuilder.CreateTable(
                name: "GroupCostOptions",
                columns: table => new
                {
                    GroupCostOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupCostOptions", x => x.GroupCostOptionId);
                    table.ForeignKey(
                        name: "FK_GroupCostOptions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupRatingOptions",
                columns: table => new
                {
                    GroupRatingOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRatingOptions", x => x.GroupRatingOptionId);
                    table.ForeignKey(
                        name: "FK_GroupRatingOptions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupVenues",
                columns: table => new
                {
                    GroupVenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VenueName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Visited = table.Column<bool>(type: "bit", nullable: false),
                    VenueTypeId = table.Column<int>(type: "int", nullable: true),
                    FoodTypeId = table.Column<int>(type: "int", nullable: true),
                    GroupRatingOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GroupCostOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupVenues", x => x.GroupVenueId);
                    table.ForeignKey(
                        name: "FK_GroupVenues_GroupCostOptions_GroupCostOptionId",
                        column: x => x.GroupCostOptionId,
                        principalTable: "GroupCostOptions",
                        principalColumn: "GroupCostOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupVenues_GroupRatingOptions_GroupRatingOptionId",
                        column: x => x.GroupRatingOptionId,
                        principalTable: "GroupRatingOptions",
                        principalColumn: "GroupRatingOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupVenues_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "GroupCostOptions",
                columns: new[] { "GroupCostOptionId", "CreatedBy", "CreatedOn", "DisplayOrder", "GroupId", "Label", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, "Cheap", null, null },
                    { new Guid("00000000-0000-0000-0001-000000000002"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, "Reasonable", null, null },
                    { new Guid("00000000-0000-0000-0001-000000000003"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, "Pricey", null, null }
                });

            migrationBuilder.InsertData(
                table: "GroupRatingOptions",
                columns: new[] { "GroupRatingOptionId", "CreatedBy", "CreatedOn", "DisplayOrder", "GroupId", "Label", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, "Great", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, "Good", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000003"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, "Average", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, "Poor", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupCostOptions_GroupId",
                table: "GroupCostOptions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRatingOptions_GroupId",
                table: "GroupRatingOptions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupVenues_GroupCostOptionId",
                table: "GroupVenues",
                column: "GroupCostOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupVenues_GroupId",
                table: "GroupVenues",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupVenues_GroupRatingOptionId",
                table: "GroupVenues",
                column: "GroupRatingOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupVenues");

            migrationBuilder.DropTable(
                name: "GroupCostOptions");

            migrationBuilder.DropTable(
                name: "GroupRatingOptions");

            migrationBuilder.CreateTable(
                name: "GroupCostOverrides",
                columns: table => new
                {
                    GroupCostOverrideId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Costs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupCostOverrides", x => x.GroupCostOverrideId);
                    table.ForeignKey(
                        name: "FK_GroupCostOverrides_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupCosts",
                columns: table => new
                {
                    GroupCostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Costs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupCosts", x => x.GroupCostId);
                });

            migrationBuilder.CreateTable(
                name: "GroupRatingOverrides",
                columns: table => new
                {
                    GroupRatingOverrideId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ratings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRatingOverrides", x => x.GroupRatingOverrideId);
                    table.ForeignKey(
                        name: "FK_GroupRatingOverrides_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupRatings",
                columns: table => new
                {
                    GroupRatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ratings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRatings", x => x.GroupRatingId);
                });

            migrationBuilder.InsertData(
                table: "GroupCosts",
                columns: new[] { "GroupCostId", "Costs", "CreatedBy", "CreatedOn", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "Cheap|Reasonable|Pricey", new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null });

            migrationBuilder.InsertData(
                table: "GroupRatings",
                columns: new[] { "GroupRatingId", "CreatedBy", "CreatedOn", "Ratings", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Great|Good|Average|Poor", null, null });

            migrationBuilder.CreateIndex(
                name: "IX_GroupCostOverrides_GroupId",
                table: "GroupCostOverrides",
                column: "GroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupRatingOverrides_GroupId",
                table: "GroupRatingOverrides",
                column: "GroupId",
                unique: true);
        }
    }
}
