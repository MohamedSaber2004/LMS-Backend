using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHelpCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HelpCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HelpArticles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpArticles_HelpCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "HelpCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HelpArticles_CategoryId",
                table: "HelpArticles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpArticles_CategoryId_Order",
                table: "HelpArticles",
                columns: new[] { "CategoryId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_HelpArticles_IsDeleted",
                table: "HelpArticles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_HelpCategories_IsDeleted",
                table: "HelpCategories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_HelpCategories_Name",
                table: "HelpCategories",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HelpCategories_Order",
                table: "HelpCategories",
                column: "Order");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HelpArticles");

            migrationBuilder.DropTable(
                name: "HelpCategories");
        }
    }
}
