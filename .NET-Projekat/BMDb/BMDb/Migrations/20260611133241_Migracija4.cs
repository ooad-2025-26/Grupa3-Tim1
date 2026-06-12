using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMDb.Migrations
{
    /// <inheritdoc />
    public partial class Migracija4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OsobaId",
                table: "OsobaZanr",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "brojanjeOglasa",
                table: "Oglas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OsobaZanr_OsobaId",
                table: "OsobaZanr",
                column: "OsobaId");

            migrationBuilder.CreateIndex(
                name: "IX_OsobaZanr_ZanrId",
                table: "OsobaZanr",
                column: "ZanrId");

            migrationBuilder.CreateIndex(
                name: "IX_EntertainmentZanr_EntertainmentId",
                table: "EntertainmentZanr",
                column: "EntertainmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EntertainmentZanr_ZanrId",
                table: "EntertainmentZanr",
                column: "ZanrId");

            migrationBuilder.AddForeignKey(
                name: "FK_EntertainmentZanr_Entertainment_EntertainmentId",
                table: "EntertainmentZanr",
                column: "EntertainmentId",
                principalTable: "Entertainment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntertainmentZanr_Zanr_ZanrId",
                table: "EntertainmentZanr",
                column: "ZanrId",
                principalTable: "Zanr",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OsobaZanr_Osoba_OsobaId",
                table: "OsobaZanr",
                column: "OsobaId",
                principalTable: "Osoba",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OsobaZanr_Zanr_ZanrId",
                table: "OsobaZanr",
                column: "ZanrId",
                principalTable: "Zanr",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntertainmentZanr_Entertainment_EntertainmentId",
                table: "EntertainmentZanr");

            migrationBuilder.DropForeignKey(
                name: "FK_EntertainmentZanr_Zanr_ZanrId",
                table: "EntertainmentZanr");

            migrationBuilder.DropForeignKey(
                name: "FK_OsobaZanr_Osoba_OsobaId",
                table: "OsobaZanr");

            migrationBuilder.DropForeignKey(
                name: "FK_OsobaZanr_Zanr_ZanrId",
                table: "OsobaZanr");

            migrationBuilder.DropIndex(
                name: "IX_OsobaZanr_OsobaId",
                table: "OsobaZanr");

            migrationBuilder.DropIndex(
                name: "IX_OsobaZanr_ZanrId",
                table: "OsobaZanr");

            migrationBuilder.DropIndex(
                name: "IX_EntertainmentZanr_EntertainmentId",
                table: "EntertainmentZanr");

            migrationBuilder.DropIndex(
                name: "IX_EntertainmentZanr_ZanrId",
                table: "EntertainmentZanr");

            migrationBuilder.DropColumn(
                name: "brojanjeOglasa",
                table: "Oglas");

            migrationBuilder.AlterColumn<string>(
                name: "OsobaId",
                table: "OsobaZanr",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
