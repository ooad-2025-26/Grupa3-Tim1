using BMDb.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMDb.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260611170000_FixSerijaSezonaPosterColumnTypes")]
    public partial class FixSerijaSezonaPosterColumnTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PosterSezone",
                table: "Sezona",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Sezona] SET [PosterSezone] = N'0' WHERE TRY_CONVERT(int, [PosterSezone]) IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "PosterSezone",
                table: "Sezona",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
