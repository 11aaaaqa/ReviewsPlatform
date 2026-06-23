using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReviewMicroservice.Api.Migrations
{
    /// <inheritdoc />
    public partial class add_comment_replies_table_and_update_comments_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ReplyToCommentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ReplyToCommentId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "ReplyToCommentId",
                table: "Comments",
                newName: "ParentCommentId");

            migrationBuilder.AddColumn<int>(
                name: "RepliesCount",
                table: "Comments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CommentReplies",
                columns: table => new
                {
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RepliedId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentReplies", x => new { x.ParentId, x.RepliedId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentReplies");

            migrationBuilder.DropColumn(
                name: "RepliesCount",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "ParentCommentId",
                table: "Comments",
                newName: "ReplyToCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ReplyToCommentId",
                table: "Comments",
                column: "ReplyToCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ReplyToCommentId",
                table: "Comments",
                column: "ReplyToCommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
