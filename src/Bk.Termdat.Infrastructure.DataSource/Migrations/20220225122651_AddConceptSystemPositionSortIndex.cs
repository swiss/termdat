using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddConceptSystemPositionSortIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sort_index",
                table: "concept_system_positions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sort_index",
                table: "concept_system_positions",
                schema: "data");
        }
    }
}
