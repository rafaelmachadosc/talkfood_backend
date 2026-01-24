using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryEntityAddCategoryString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Adicionar coluna Category na tabela products
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "products",
                type: "text",
                nullable: false,
                defaultValue: "");

            // 2. Migrar dados de categories.name para products.category
            migrationBuilder.Sql(@"
                UPDATE products 
                SET ""Category"" = (
                    SELECT c.""Name"" 
                    FROM categories c 
                    WHERE c.""Id"" = products.""CategoryId""
                )
                WHERE ""CategoryId"" IS NOT NULL;
            ");

            // 3. Remover foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_products_categories_CategoryId",
                table: "products");

            // 4. Remover índice
            migrationBuilder.DropIndex(
                name: "IX_products_CategoryId",
                table: "products");

            // 5. Remover coluna CategoryId
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "products");

            // 6. Remover tabela categories
            migrationBuilder.DropTable(
                name: "categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recriar tabela categories
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                });

            // Adicionar coluna CategoryId
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Recriar índice
            migrationBuilder.CreateIndex(
                name: "IX_products_CategoryId",
                table: "products",
                column: "CategoryId");

            // Recriar foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_products_categories_CategoryId",
                table: "products",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Remover coluna Category
            migrationBuilder.DropColumn(
                name: "Category",
                table: "products");
        }
    }
}
