using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CategoryMicroservice.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ReviewsCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subcategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ReviewsCount = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subcategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subcategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "ReviewsCount" },
                values: new object[,]
                {
                    { new Guid("15c2d4a7-9dac-44b7-acb9-f493ccdfc99d"), "Детское", 0 },
                    { new Guid("431b57c0-315f-42a7-b699-647af7843273"), "Красота и здоровье", 0 },
                    { new Guid("8697d5b5-e2a4-4ed8-9723-81d981ca4d51"), "Техника", 0 },
                    { new Guid("a4953008-2fc2-4ed2-9ab8-302bdd2f9e69"), "Разное", 0 },
                    { new Guid("dee670bb-90d0-443a-b77f-347e85e59cec"), "Авто", 0 }
                });

            migrationBuilder.InsertData(
                table: "Subcategories",
                columns: new[] { "Id", "CategoryId", "Name", "ReviewsCount" },
                values: new object[,]
                {
                    { new Guid("0dda864d-96de-482a-85e7-693611dc211c"), new Guid("dee670bb-90d0-443a-b77f-347e85e59cec"), "Запчасти", 0 },
                    { new Guid("181ddb8a-f92c-4518-abe2-b514e354df81"), new Guid("15c2d4a7-9dac-44b7-acb9-f493ccdfc99d"), "Детские игрушки", 0 },
                    { new Guid("2aabcf86-3ccf-4382-ad2a-dc996353f3a8"), new Guid("a4953008-2fc2-4ed2-9ab8-302bdd2f9e69"), "Ремонт", 0 },
                    { new Guid("2f6d561b-072d-405d-ae1c-2312dbdc6f19"), new Guid("15c2d4a7-9dac-44b7-acb9-f493ccdfc99d"), "Детская обувь", 0 },
                    { new Guid("3f76067a-fa50-44f4-99cd-d10bc32d5387"), new Guid("8697d5b5-e2a4-4ed8-9723-81d981ca4d51"), "Для дома", 0 },
                    { new Guid("426327e2-9ca1-489c-8adb-098a4cf3ddac"), new Guid("15c2d4a7-9dac-44b7-acb9-f493ccdfc99d"), "Одежда для мальчиков", 0 },
                    { new Guid("45ba3685-954e-4ee8-b9e5-c26101c1cc08"), new Guid("dee670bb-90d0-443a-b77f-347e85e59cec"), "Автомобили", 0 },
                    { new Guid("485d0a5a-4361-41c4-a22c-71ffb0aabfc6"), new Guid("431b57c0-315f-42a7-b699-647af7843273"), "Спортивные товары", 0 },
                    { new Guid("63114816-16bf-43fd-bfa4-c0434959b23c"), new Guid("431b57c0-315f-42a7-b699-647af7843273"), "Косметические аксессуары", 0 },
                    { new Guid("6dcad3c5-0b4e-4c56-b603-a9c1544798b7"), new Guid("a4953008-2fc2-4ed2-9ab8-302bdd2f9e69"), "Обувь", 0 },
                    { new Guid("72cea28b-9495-41e2-878f-1f1a58d25a43"), new Guid("431b57c0-315f-42a7-b699-647af7843273"), "Спорт/фитнес-клубы", 0 },
                    { new Guid("769fcc5d-4a3a-4436-86a7-3dfbb9bf3de0"), new Guid("8697d5b5-e2a4-4ed8-9723-81d981ca4d51"), "Компьютеры", 0 },
                    { new Guid("76fab645-53b0-4714-8243-a8e3dbad8500"), new Guid("431b57c0-315f-42a7-b699-647af7843273"), "Товары для здоровья", 0 },
                    { new Guid("7c3c149a-d162-4e64-9b9b-ca62066387fb"), new Guid("431b57c0-315f-42a7-b699-647af7843273"), "Красота, здоровье - разное", 0 },
                    { new Guid("9470f494-f597-4f6f-b1ec-0391afeff754"), new Guid("a4953008-2fc2-4ed2-9ab8-302bdd2f9e69"), "Одежда", 0 },
                    { new Guid("9b49d48c-3024-4194-8b51-638fea13f64f"), new Guid("431b57c0-315f-42a7-b699-647af7843273"), "Похудение", 0 },
                    { new Guid("a30799d9-653a-4181-8592-1c468634b788"), new Guid("15c2d4a7-9dac-44b7-acb9-f493ccdfc99d"), "Одежда для девочек", 0 },
                    { new Guid("adea119a-4d4b-4358-b4e7-25c4146a7a24"), new Guid("15c2d4a7-9dac-44b7-acb9-f493ccdfc99d"), "Детское - разное", 0 },
                    { new Guid("c59369e4-9f1c-4265-8100-0477c14640d1"), new Guid("dee670bb-90d0-443a-b77f-347e85e59cec"), "Авто - разное", 0 },
                    { new Guid("cb45da5e-bc7b-4687-bba8-8ea0f637210c"), new Guid("a4953008-2fc2-4ed2-9ab8-302bdd2f9e69"), "Компьютерные игры", 0 },
                    { new Guid("d5d44835-e8d8-44c5-8035-80d2b6ff8ca1"), new Guid("8697d5b5-e2a4-4ed8-9723-81d981ca4d51"), "Техника - разное", 0 },
                    { new Guid("d6453ea4-5f00-4df3-9eb0-2baafe26aaf8"), new Guid("a4953008-2fc2-4ed2-9ab8-302bdd2f9e69"), "Другое", 0 },
                    { new Guid("dfb3cedf-63aa-4004-bfc1-eaec31d3467c"), new Guid("431b57c0-315f-42a7-b699-647af7843273"), "Ухаживающая косметика", 0 },
                    { new Guid("e53a33f9-f947-4d75-8aa9-b29a5e926f20"), new Guid("8697d5b5-e2a4-4ed8-9723-81d981ca4d51"), "Аксессуары для техники", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subcategories_CategoryId",
                table: "Subcategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Subcategories_Name",
                table: "Subcategories",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subcategories");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
