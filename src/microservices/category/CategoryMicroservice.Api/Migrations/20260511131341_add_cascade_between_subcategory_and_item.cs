using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CategoryMicroservice.Api.Migrations
{
    /// <inheritdoc />
    public partial class add_cascade_between_subcategory_and_item : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Items_SubcategoryId",
                table: "Items",
                column: "SubcategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Subcategories_SubcategoryId",
                table: "Items",
                column: "SubcategoryId",
                principalTable: "Subcategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Subcategories_SubcategoryId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_SubcategoryId",
                table: "Items");
        }
    }
}
