using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddNavPropGroupCreatedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Groups_CreatedBy",
                table: "Groups",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Users_CreatedBy",
                table: "Groups",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Users_CreatedBy",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_CreatedBy",
                table: "Groups");
        }
    }
}
