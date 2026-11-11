using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionPaymentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Sessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAtUtc",
                table: "Sessions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "PaidAtUtc",
                table: "Sessions");
        }
    }
}
