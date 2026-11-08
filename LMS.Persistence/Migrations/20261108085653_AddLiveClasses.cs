using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiveRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HostUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_LiveRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveRooms_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveRooms_Users_HostUserId",
                        column: x => x.HostUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LiveRoomParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    AgoraUid = table.Column<long>(type: "bigint", nullable: false),
                    JoinedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_LiveRoomParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveRoomParticipants_LiveRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "LiveRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveRoomParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveRoomParticipants_IsDeleted",
                table: "LiveRoomParticipants",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_LiveRoomParticipants_RoomId",
                table: "LiveRoomParticipants",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveRoomParticipants_RoomId_UserId",
                table: "LiveRoomParticipants",
                columns: new[] { "RoomId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_LiveRoomParticipants_UserId",
                table: "LiveRoomParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveRooms_ChannelName",
                table: "LiveRooms",
                column: "ChannelName");

            migrationBuilder.CreateIndex(
                name: "IX_LiveRooms_HostUserId",
                table: "LiveRooms",
                column: "HostUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveRooms_IsDeleted",
                table: "LiveRooms",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_LiveRooms_SessionId",
                table: "LiveRooms",
                column: "SessionId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LiveRooms_Status",
                table: "LiveRooms",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveRoomParticipants");

            migrationBuilder.DropTable(
                name: "LiveRooms");
        }
    }
}
