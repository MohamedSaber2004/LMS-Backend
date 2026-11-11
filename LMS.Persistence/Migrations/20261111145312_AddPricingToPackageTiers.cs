using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingToPackageTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PricePerHour",
                table: "PackageTiers",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "PackageTiers",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerHour",
                table: "PackageTiers");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "PackageTiers");
        }
    }
}
