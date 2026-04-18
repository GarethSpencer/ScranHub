using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodTypeVenueType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_GroupCostOptions_GroupCostOptionId",
                table: "GroupVenues");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_GroupRatingOptions_GroupRatingOptionId",
                table: "GroupVenues");

            migrationBuilder.DropTable(
                name: "GroupCostOptions");

            migrationBuilder.DropTable(
                name: "GroupRatingOptions");

            migrationBuilder.DropColumn(
                name: "FoodTypeId",
                table: "GroupVenues");

            migrationBuilder.DropColumn(
                name: "VenueTypeId",
                table: "GroupVenues");

            migrationBuilder.RenameColumn(
                name: "GroupRatingOptionId",
                table: "GroupVenues",
                newName: "VenueTypeOptionId");

            migrationBuilder.RenameColumn(
                name: "GroupCostOptionId",
                table: "GroupVenues",
                newName: "RatingOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupVenues_GroupRatingOptionId",
                table: "GroupVenues",
                newName: "IX_GroupVenues_VenueTypeOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupVenues_GroupCostOptionId",
                table: "GroupVenues",
                newName: "IX_GroupVenues_RatingOptionId");

            migrationBuilder.AddColumn<Guid>(
                name: "CostOptionId",
                table: "GroupVenues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FoodTypeOptionId",
                table: "GroupVenues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CostOptions",
                columns: table => new
                {
                    CostOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_CostOptions", x => x.CostOptionId);
                    table.ForeignKey(
                        name: "FK_CostOptions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FoodTypeOptions",
                columns: table => new
                {
                    FoodTypeOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_FoodTypeOptions", x => x.FoodTypeOptionId);
                    table.ForeignKey(
                        name: "FK_FoodTypeOptions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RatingOptions",
                columns: table => new
                {
                    RatingOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_RatingOptions", x => x.RatingOptionId);
                    table.ForeignKey(
                        name: "FK_RatingOptions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VenueTypeOptions",
                columns: table => new
                {
                    VenueTypeOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_VenueTypeOptions", x => x.VenueTypeOptionId);
                    table.ForeignKey(
                        name: "FK_VenueTypeOptions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CostOptions",
                columns: new[] { "CostOptionId", "CreatedBy", "CreatedOn", "DisplayOrder", "GroupId", "Label", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, "Cheap", null, null },
                    { new Guid("00000000-0000-0000-0001-000000000002"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, "Reasonable", null, null },
                    { new Guid("00000000-0000-0000-0001-000000000003"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, "Pricey", null, null }
                });

            migrationBuilder.InsertData(
                table: "FoodTypeOptions",
                columns: new[] { "FoodTypeOptionId", "CreatedBy", "CreatedOn", "DisplayOrder", "GroupId", "Label", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, "Café", null, null },
                    { new Guid("00000000-0000-0000-0003-000000000002"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, "Burgers", null, null },
                    { new Guid("00000000-0000-0000-0003-000000000003"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, "Chicken", null, null },
                    { new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Pizza", null, null },
                    { new Guid("00000000-0000-0000-0003-000000000005"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 5, null, "Oriental", null, null },
                    { new Guid("00000000-0000-0000-0003-000000000006"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 6, null, "Other", null, null }
                });

            migrationBuilder.InsertData(
                table: "RatingOptions",
                columns: new[] { "RatingOptionId", "CreatedBy", "CreatedOn", "DisplayOrder", "GroupId", "Label", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, "Great", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, "Good", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000003"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, "Average", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Poor", null, null }
                });

            migrationBuilder.InsertData(
                table: "VenueTypeOptions",
                columns: new[] { "VenueTypeOptionId", "CreatedBy", "CreatedOn", "DisplayOrder", "GroupId", "Label", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0004-000000000001"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, "Eat-In", null, null },
                    { new Guid("00000000-0000-0000-0004-000000000002"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, "Takeaway", null, null },
                    { new Guid("00000000-0000-0000-0004-000000000003"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, "Both", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupVenues_CostOptionId",
                table: "GroupVenues",
                column: "CostOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupVenues_FoodTypeOptionId",
                table: "GroupVenues",
                column: "FoodTypeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CostOptions_GroupId",
                table: "CostOptions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodTypeOptions_GroupId",
                table: "FoodTypeOptions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RatingOptions_GroupId",
                table: "RatingOptions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueTypeOptions_GroupId",
                table: "VenueTypeOptions",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_CostOptions_CostOptionId",
                table: "GroupVenues",
                column: "CostOptionId",
                principalTable: "CostOptions",
                principalColumn: "CostOptionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_FoodTypeOptions_FoodTypeOptionId",
                table: "GroupVenues",
                column: "FoodTypeOptionId",
                principalTable: "FoodTypeOptions",
                principalColumn: "FoodTypeOptionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_RatingOptions_RatingOptionId",
                table: "GroupVenues",
                column: "RatingOptionId",
                principalTable: "RatingOptions",
                principalColumn: "RatingOptionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_VenueTypeOptions_VenueTypeOptionId",
                table: "GroupVenues",
                column: "VenueTypeOptionId",
                principalTable: "VenueTypeOptions",
                principalColumn: "VenueTypeOptionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_CostOptions_CostOptionId",
                table: "GroupVenues");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_FoodTypeOptions_FoodTypeOptionId",
                table: "GroupVenues");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_RatingOptions_RatingOptionId",
                table: "GroupVenues");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupVenues_VenueTypeOptions_VenueTypeOptionId",
                table: "GroupVenues");

            migrationBuilder.DropTable(
                name: "CostOptions");

            migrationBuilder.DropTable(
                name: "FoodTypeOptions");

            migrationBuilder.DropTable(
                name: "RatingOptions");

            migrationBuilder.DropTable(
                name: "VenueTypeOptions");

            migrationBuilder.DropIndex(
                name: "IX_GroupVenues_CostOptionId",
                table: "GroupVenues");

            migrationBuilder.DropIndex(
                name: "IX_GroupVenues_FoodTypeOptionId",
                table: "GroupVenues");

            migrationBuilder.DropColumn(
                name: "CostOptionId",
                table: "GroupVenues");

            migrationBuilder.DropColumn(
                name: "FoodTypeOptionId",
                table: "GroupVenues");

            migrationBuilder.RenameColumn(
                name: "VenueTypeOptionId",
                table: "GroupVenues",
                newName: "GroupRatingOptionId");

            migrationBuilder.RenameColumn(
                name: "RatingOptionId",
                table: "GroupVenues",
                newName: "GroupCostOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupVenues_VenueTypeOptionId",
                table: "GroupVenues",
                newName: "IX_GroupVenues_GroupRatingOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupVenues_RatingOptionId",
                table: "GroupVenues",
                newName: "IX_GroupVenues_GroupCostOptionId");

            migrationBuilder.AddColumn<int>(
                name: "FoodTypeId",
                table: "GroupVenues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VenueTypeId",
                table: "GroupVenues",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GroupCostOptions",
                columns: table => new
                {
                    GroupCostOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
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

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_GroupCostOptions_GroupCostOptionId",
                table: "GroupVenues",
                column: "GroupCostOptionId",
                principalTable: "GroupCostOptions",
                principalColumn: "GroupCostOptionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupVenues_GroupRatingOptions_GroupRatingOptionId",
                table: "GroupVenues",
                column: "GroupRatingOptionId",
                principalTable: "GroupRatingOptions",
                principalColumn: "GroupRatingOptionId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
