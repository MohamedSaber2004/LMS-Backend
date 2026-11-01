using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorApplicationHourlyRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "InstructorApplications",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_InstructorApplications_Status_HourlyRate",
                table: "InstructorApplications",
                columns: new[] { "Status", "HourlyRate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InstructorApplications_Status_HourlyRate",
                table: "InstructorApplications");

            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "InstructorApplications");
        }
    }
}
