using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeparateLearnerAndInstructorProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstructorApplications_Users_UserId",
                table: "InstructorApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorCertificates_InstructorApplications_InstructorApplicationId",
                table: "InstructorCertificates");

            migrationBuilder.CreateTable(
                name: "Instructors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: false),
                    Education = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Instructors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Instructors_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Learners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_Learners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Learners_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_IsDeleted",
                table: "Instructors",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_UserId",
                table: "Instructors",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Learners_IsDeleted",
                table: "Learners",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Learners_UserId",
                table: "Learners",
                column: "UserId",
                unique: true);

            // Teacher profile data moves out of the review record. The new profile row keeps the
            // application id so certificates and applications can be re-pointed without a lookup.
            migrationBuilder.Sql(
                """
                INSERT INTO Instructors
                    (Id, UserId, Specialization, Bio, YearsOfExperience, Education, LinkedInUrl,
                     CreatedAt, UpdatedAt, DeletedAt, CreatedBy, UpdatedBy, DeletedBy, IsDeleted, IsActive)
                SELECT Id, UserId, Specialization, Bio, YearsOfExperience, Education, LinkedInUrl,
                       CreatedAt, UpdatedAt, DeletedAt, CreatedBy, UpdatedBy, DeletedBy, IsDeleted, IsActive
                FROM InstructorApplications;
                """);

            // Student accounts get their own profile row.
            migrationBuilder.Sql(
                """
                INSERT INTO Learners (Id, UserId, CreatedAt, CreatedBy, IsDeleted, IsActive)
                SELECT NEWID(), u.Id, SYSUTCDATETIME(), 'System', CAST(0 AS bit), CAST(1 AS bit)
                FROM Users u
                INNER JOIN AspNetUserRoles userRole ON userRole.UserId = u.Id
                INNER JOIN AspNetRoles role ON role.Id = userRole.RoleId
                WHERE role.Name = 'Learner';
                """);

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "InstructorApplications",
                newName: "InstructorId");

            migrationBuilder.RenameIndex(
                name: "IX_InstructorApplications_UserId",
                table: "InstructorApplications",
                newName: "IX_InstructorApplications_InstructorId");

            migrationBuilder.Sql(
                """
                UPDATE InstructorApplications SET InstructorId = Id;
                """);

            // InstructorCertificates.InstructorApplicationId already matches the profile id copied above.
            migrationBuilder.RenameColumn(
                name: "InstructorApplicationId",
                table: "InstructorCertificates",
                newName: "InstructorId");

            migrationBuilder.RenameIndex(
                name: "IX_InstructorCertificates_InstructorApplicationId",
                table: "InstructorCertificates",
                newName: "IX_InstructorCertificates_InstructorId");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "InstructorApplications");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "InstructorApplications");

            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "InstructorApplications");

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "InstructorApplications");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "InstructorApplications");

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorApplications_Instructors_InstructorId",
                table: "InstructorApplications",
                column: "InstructorId",
                principalTable: "Instructors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorCertificates_Instructors_InstructorId",
                table: "InstructorCertificates",
                column: "InstructorId",
                principalTable: "Instructors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstructorApplications_Instructors_InstructorId",
                table: "InstructorApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorCertificates_Instructors_InstructorId",
                table: "InstructorCertificates");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "InstructorApplications",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "InstructorApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "InstructorApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "InstructorApplications",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                table: "InstructorApplications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE application
                SET application.Specialization = instructor.Specialization,
                    application.Bio = instructor.Bio,
                    application.YearsOfExperience = instructor.YearsOfExperience,
                    application.Education = instructor.Education,
                    application.LinkedInUrl = instructor.LinkedInUrl
                FROM InstructorApplications application
                INNER JOIN Instructors instructor ON instructor.Id = application.InstructorId;
                """);

            // Applications and certificates are keyed by the profile id, which only this migration
            // generated; accounts created after it have no application id to fall back on.
            migrationBuilder.Sql(
                """
                UPDATE certificate
                SET certificate.InstructorId = application.Id
                FROM InstructorCertificates certificate
                INNER JOIN Instructors instructor ON instructor.Id = certificate.InstructorId
                INNER JOIN InstructorApplications application ON application.InstructorId = instructor.Id;

                UPDATE application
                SET application.InstructorId = instructor.UserId
                FROM InstructorApplications application
                INNER JOIN Instructors instructor ON instructor.Id = application.InstructorId;
                """);

            migrationBuilder.DropTable(
                name: "Learners");

            migrationBuilder.DropTable(
                name: "Instructors");

            migrationBuilder.RenameColumn(
                name: "InstructorId",
                table: "InstructorCertificates",
                newName: "InstructorApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_InstructorCertificates_InstructorId",
                table: "InstructorCertificates",
                newName: "IX_InstructorCertificates_InstructorApplicationId");

            migrationBuilder.RenameColumn(
                name: "InstructorId",
                table: "InstructorApplications",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_InstructorApplications_InstructorId",
                table: "InstructorApplications",
                newName: "IX_InstructorApplications_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorApplications_Users_UserId",
                table: "InstructorApplications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorCertificates_InstructorApplications_InstructorApplicationId",
                table: "InstructorCertificates",
                column: "InstructorApplicationId",
                principalTable: "InstructorApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
