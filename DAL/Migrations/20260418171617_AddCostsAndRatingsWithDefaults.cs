using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCostsAndRatingsWithDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupCostOverrides",
                columns: table => new
                {
                    GroupCostOverrideId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Costs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    Ratings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    Ratings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRatings", x => x.GroupRatingId);
                });

            migrationBuilder.InsertData(
                table: "GroupCosts",
                columns: new[] { "GroupCostId", "Costs", "CreatedBy", "CreatedOn", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "Cheap|Reasonable|Pricey", new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "GroupRatings",
                columns: new[] { "GroupRatingId", "CreatedBy", "CreatedOn", "Ratings", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Great|Good|Average|Poor", new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupCostOverrides");

            migrationBuilder.DropTable(
                name: "GroupCosts");

            migrationBuilder.DropTable(
                name: "GroupRatingOverrides");

            migrationBuilder.DropTable(
                name: "GroupRatings");
        }
    }
}
