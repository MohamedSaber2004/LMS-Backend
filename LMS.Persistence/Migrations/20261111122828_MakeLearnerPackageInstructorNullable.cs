using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeLearnerPackageInstructorNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LearnerPackages_LearnerId_InstructorId_IsActive",
                table: "LearnerPackages");

            migrationBuilder.AlterColumn<Guid>(
                name: "InstructorId",
                table: "LearnerPackages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerPackages_LearnerId_IsActive",
                table: "LearnerPackages",
                columns: new[] { "LearnerId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LearnerPackages_LearnerId_IsActive",
                table: "LearnerPackages");

            migrationBuilder.AlterColumn<Guid>(
                name: "InstructorId",
                table: "LearnerPackages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearnerPackages_LearnerId_InstructorId_IsActive",
                table: "LearnerPackages",
                columns: new[] { "LearnerId", "InstructorId", "IsActive" });
        }
    }
}
