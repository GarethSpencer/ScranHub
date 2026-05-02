using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AmendUserFriendStatusField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "UserFriends");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "UserFriends",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserFriends");

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "UserFriends",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
