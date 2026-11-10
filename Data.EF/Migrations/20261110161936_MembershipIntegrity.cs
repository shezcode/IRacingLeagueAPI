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

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Races",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Leagues",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

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

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Leagues");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_LeagueId",
                table: "Registrations",
                column: "LeagueId");
        }
    }
}
