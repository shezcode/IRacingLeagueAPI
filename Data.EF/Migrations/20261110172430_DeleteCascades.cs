using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IRacingLeague.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class DeleteCascades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Races_Leagues_LeagueId",
                table: "Races");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Leagues_LeagueId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Users_UserId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Races_RaceId",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Registrations_RegistrationId",
                table: "Results");

            migrationBuilder.AddForeignKey(
                name: "FK_Races_Leagues_LeagueId",
                table: "Races",
                column: "LeagueId",
                principalTable: "Leagues",
                principalColumn: "LeagueId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Leagues_LeagueId",
                table: "Registrations",
                column: "LeagueId",
                principalTable: "Leagues",
                principalColumn: "LeagueId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Users_UserId",
                table: "Registrations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Races_RaceId",
                table: "Results",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Registrations_RegistrationId",
                table: "Results",
                column: "RegistrationId",
                principalTable: "Registrations",
                principalColumn: "RegistrationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Races_Leagues_LeagueId",
                table: "Races");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Leagues_LeagueId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Users_UserId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Races_RaceId",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Registrations_RegistrationId",
                table: "Results");

            migrationBuilder.AddForeignKey(
                name: "FK_Races_Leagues_LeagueId",
                table: "Races",
                column: "LeagueId",
                principalTable: "Leagues",
                principalColumn: "LeagueId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Leagues_LeagueId",
                table: "Registrations",
                column: "LeagueId",
                principalTable: "Leagues",
                principalColumn: "LeagueId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Users_UserId",
                table: "Registrations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Races_RaceId",
                table: "Results",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Registrations_RegistrationId",
                table: "Results",
                column: "RegistrationId",
                principalTable: "Registrations",
                principalColumn: "RegistrationId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
