using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddConceptSystemPositionEntryOnDeleteSetNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_entries_concept_system_positions_concept_system_position_id",
                table: "entries",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_entries_concept_system_positions_concept_system_position_id",
                table: "entries",
                column: "concept_system_position_id",
                principalTable: "concept_system_positions",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.SetNull,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_entries_concept_system_positions_concept_system_position_id",
                table: "entries",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_entries_concept_system_positions_concept_system_position_id",
                table: "entries",
                column: "concept_system_position_id",
                principalTable: "concept_system_positions",
                principalColumn: "id",
                principalSchema: "data",
                schema: "data");
        }
    }
}
