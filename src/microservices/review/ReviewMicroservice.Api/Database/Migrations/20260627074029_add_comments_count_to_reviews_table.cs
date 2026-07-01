using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReviewMicroservice.Api.Migrations
{
    /// <inheritdoc />
    public partial class add_comments_count_to_reviews_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommentsCount",
                table: "Reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentsCount",
                table: "Reviews");
        }
    }
}
