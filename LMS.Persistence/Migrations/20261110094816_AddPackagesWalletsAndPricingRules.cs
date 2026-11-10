using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPackagesWalletsAndPricingRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AttendanceConfirmedAtUtc",
                table: "Sessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRateSnapshot",
                table: "Sessions",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Sessions",
                type: "int",
                nullable: false,
                defaultValue: 50);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRateSnapshot",
                table: "Sessions",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsAttendanceConfirmed",
                table: "Sessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrial",
                table: "Sessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate",
                table: "Instructors",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 20.00m);

            migrationBuilder.AddColumn<bool>(
                name: "IsNativeSpeaker",
                table: "Instructors",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Languages",
                table: "Instructors",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "Arabic");

            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "Instructors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LearnerPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TotalHours = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    RemainingHours = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    HourlyRateSnapshot = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearnerPackages_Instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearnerPackages_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherWallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    PendingBalance = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    TotalEarned = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
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
                    table.PrimaryKey("PK_TeacherWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherWallets_Instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherWalletTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_TeacherWalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherWalletTransactions_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherWalletTransactions_TeacherWallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "TeacherWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearnerPackages_InstructorId",
                table: "LearnerPackages",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerPackages_IsDeleted",
                table: "LearnerPackages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerPackages_LearnerId",
                table: "LearnerPackages",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerPackages_LearnerId_InstructorId_IsActive",
                table: "LearnerPackages",
                columns: new[] { "LearnerId", "InstructorId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherWallets_InstructorId",
                table: "TeacherWallets",
                column: "InstructorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherWallets_IsDeleted",
                table: "TeacherWallets",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherWalletTransactions_IsDeleted",
                table: "TeacherWalletTransactions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherWalletTransactions_SessionId",
                table: "TeacherWalletTransactions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherWalletTransactions_WalletId",
                table: "TeacherWalletTransactions",
                column: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearnerPackages");

            migrationBuilder.DropTable(
                name: "TeacherWalletTransactions");

            migrationBuilder.DropTable(
                name: "TeacherWallets");

            migrationBuilder.DropColumn(
                name: "AttendanceConfirmedAtUtc",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CommissionRateSnapshot",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "HourlyRateSnapshot",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "IsAttendanceConfirmed",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "IsTrial",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CommissionRate",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "IsNativeSpeaker",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "Languages",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "Instructors");
        }
    }
}
