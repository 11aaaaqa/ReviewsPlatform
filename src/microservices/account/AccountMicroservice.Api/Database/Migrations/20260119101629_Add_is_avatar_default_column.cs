using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountMicroservice.Api.Migrations
{
    /// <inheritdoc />
    public partial class Add_is_avatar_default_column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvatarDefault",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvatarDefault",
                table: "Users");
        }
    }
}
