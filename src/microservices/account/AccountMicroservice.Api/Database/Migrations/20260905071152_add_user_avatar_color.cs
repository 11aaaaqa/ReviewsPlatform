using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountMicroservice.Api.Migrations
{
    /// <inheritdoc />
    public partial class add_user_avatar_color : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultAvatarColorHex",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "#FF8B008B");

            migrationBuilder.CreateIndex(
                name: "IX_UserEmailTokens_Token",
                table: "UserEmailTokens",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserEmailTokens_Token",
                table: "UserEmailTokens");

            migrationBuilder.DropColumn(
                name: "DefaultAvatarColorHex",
                table: "Users");
        }
    }
}
