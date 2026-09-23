using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class CleanUpConceptSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_entries_concept_systems_concept_system_id",
                table: "entries",
                schema: "data");

            migrationBuilder.DropIndex(
                name: "ix_entries_concept_system_id",
                table: "entries",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "concept_system_id",
                table: "entries",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "concept_system_id",
                table: "entries",
                type: "integer",
                nullable: true,
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_entries_concept_system_id",
                table: "entries",
                column: "concept_system_id",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_entries_concept_systems_concept_system_id",
                table: "entries",
                column: "concept_system_id",
                principalTable: "concept_systems",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");
        }
    }
}
