using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IRacingLeague.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class MembershipIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Registrations_LeagueId",
                table: "Registrations");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_LeagueId_CarNumber",
                table: "Registrations",
                columns: new[] { "LeagueId", "CarNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_LeagueId_UserId",
                table: "Registrations",
                columns: new[] { "LeagueId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Registrations_LeagueId_CarNumber",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_LeagueId_UserId",
                table: "Registrations");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_LeagueId",
                table: "Registrations",
                column: "LeagueId");
        }
    }
}
