using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CategoryMicroservice.Api.Migrations
{
    /// <inheritdoc />
    public partial class Add_item_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: true),
                    GeneralEstimation = table.Column<double>(type: "double precision", nullable: false),
                    ReviewsCount = table.Column<int>(type: "integer", nullable: false),
                    Picture = table.Column<byte[]>(type: "bytea", nullable: false),
                    SubcategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}
