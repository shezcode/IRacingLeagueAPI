using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IRacingLeague.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class ClassificationPointsStandings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Results_RaceId",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "Registrations");

            migrationBuilder.AddColumn<bool>(
                name: "FastestLap",
                table: "Results",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FastestLapBonus",
                table: "Leagues",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int[]>(
                name: "PointsTable",
                table: "Leagues",
                type: "integer[]",
                nullable: false,
                defaultValue: new[] { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Results_RaceId_Position",
                table: "Results",
                columns: new[] { "RaceId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Results_RaceId_RegistrationId",
                table: "Results",
                columns: new[] { "RaceId", "RegistrationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Results_RaceId_Position",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Results_RaceId_RegistrationId",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "FastestLap",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "FastestLapBonus",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "PointsTable",
                table: "Leagues");

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "Registrations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Results_RaceId",
                table: "Results",
                column: "RaceId");
        }
    }
}
