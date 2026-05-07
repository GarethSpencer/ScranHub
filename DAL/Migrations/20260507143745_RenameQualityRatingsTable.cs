using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameQualityRatingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RatingUserRatings");

            migrationBuilder.DropTable(
                name: "RatingOptions");

            migrationBuilder.CreateTable(
                name: "QualityOptions",
                columns: table => new
                {
                    QualityOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityOptions", x => x.QualityOptionId);
                    table.ForeignKey(
                        name: "FK_QualityOptions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualityRatings",
                columns: table => new
                {
                    QualityRatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupVenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QualityOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityRatings", x => x.QualityRatingId);
                    table.ForeignKey(
                        name: "FK_QualityRatings_GroupVenues_GroupVenueId",
                        column: x => x.GroupVenueId,
                        principalTable: "GroupVenues",
                        principalColumn: "GroupVenueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityRatings_QualityOptions_QualityOptionId",
                        column: x => x.QualityOptionId,
                        principalTable: "QualityOptions",
                        principalColumn: "QualityOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityRatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "QualityOptions",
                columns: new[] { "QualityOptionId", "CreatedBy", "CreatedOn", "DisplayOrder", "GroupId", "Label", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, "Great", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, "Good", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000003"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, "Average", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Poor", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualityOptions_GroupId",
                table: "QualityOptions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityRatings_GroupVenueId",
                table: "QualityRatings",
                column: "GroupVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityRatings_QualityOptionId",
                table: "QualityRatings",
                column: "QualityOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityRatings_UserId",
                table: "QualityRatings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualityRatings");

            migrationBuilder.DropTable(
                name: "QualityOptions");

            migrationBuilder.CreateTable(
                name: "RatingOptions",
                columns: table => new
                {
                    RatingOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingOptions", x => x.RatingOptionId);
                    table.ForeignKey(
                        name: "FK_RatingOptions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RatingUserRatings",
                columns: table => new
                {
                    RatingUserRatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupVenueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RatingOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingUserRatings", x => x.RatingUserRatingId);
                    table.ForeignKey(
                        name: "FK_RatingUserRatings_GroupVenues_GroupVenueId",
                        column: x => x.GroupVenueId,
                        principalTable: "GroupVenues",
                        principalColumn: "GroupVenueId",
                        onDelete: ReferentialAction.Cascade);
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
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_RatingOptions_GroupId",
                table: "RatingOptions",
                column: "GroupId");

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
    }
}
