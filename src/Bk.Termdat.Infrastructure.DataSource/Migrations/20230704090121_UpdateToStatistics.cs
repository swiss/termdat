using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class UpdateToStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_statistic",
                table: "statistic",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "statistic",
                newName: "statistics",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_statistics",
                table: "statistics",
                column: "id",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_statistics",
                table: "statistics",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "statistics",
                newName: "statistic",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_statistic",
                table: "statistic",
                column: "id",
                schema: "data");
        }
    }
}
