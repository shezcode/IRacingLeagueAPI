using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IRacingLeague.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class LeagueInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeagueInvites",
                columns: table => new
                {
                    LeagueInviteId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeagueId = table.Column<int>(type: "integer", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsedByUserId = table.Column<int>(type: "integer", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueInvites", x => x.LeagueInviteId);
                    table.ForeignKey(
                        name: "FK_LeagueInvites_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeagueInvites_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LeagueInvites_Users_UsedByUserId",
                        column: x => x.UsedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueInvites_CreatedByUserId",
                table: "LeagueInvites",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueInvites_LeagueId_CreatedAt",
                table: "LeagueInvites",
                columns: new[] { "LeagueId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueInvites_TokenHash",
                table: "LeagueInvites",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeagueInvites_UsedByUserId",
                table: "LeagueInvites",
                column: "UsedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeagueInvites");
        }
    }
}
