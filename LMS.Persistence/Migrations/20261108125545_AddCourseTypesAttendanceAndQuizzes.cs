using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseTypesAttendanceAndQuizzes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LessonId",
                table: "Sessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "QuizQuestions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "QuizQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "QuizAttempts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "QuizAttempts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VideoUrl",
                table: "Lessons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SessionAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    JoinedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeftAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_SessionAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionAttendances_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionAttendances_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_LessonId",
                table: "Sessions",
                column: "LessonId",
                unique: true,
                filter: "[LessonId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_CourseId",
                table: "QuizQuestions",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_CourseId_Order",
                table: "QuizQuestions",
                columns: new[] { "CourseId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_CourseId_LearnerId",
                table: "QuizAttempts",
                columns: new[] { "CourseId", "LearnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Type",
                table: "Courses",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAttendances_IsDeleted",
                table: "SessionAttendances",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAttendances_LearnerId",
                table: "SessionAttendances",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAttendances_SessionId_LearnerId",
                table: "SessionAttendances",
                columns: new[] { "SessionId", "LearnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionAttendances_Status",
                table: "SessionAttendances",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Courses_CourseId",
                table: "QuizAttempts",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestions_Courses_CourseId",
                table: "QuizQuestions",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Lessons_LessonId",
                table: "Sessions",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Courses_CourseId",
                table: "QuizAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestions_Courses_CourseId",
                table: "QuizQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Lessons_LessonId",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "SessionAttendances");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_LessonId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_QuizQuestions_CourseId",
                table: "QuizQuestions");

            migrationBuilder.DropIndex(
                name: "IX_QuizQuestions_CourseId_Order",
                table: "QuizQuestions");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_CourseId_LearnerId",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Type",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Courses");

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "QuizQuestions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "QuizAttempts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VideoUrl",
                table: "Lessons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
