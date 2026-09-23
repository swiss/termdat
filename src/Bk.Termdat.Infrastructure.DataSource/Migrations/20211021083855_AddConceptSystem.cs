using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddConceptSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "concept_system_id",
                table: "entries",
                type: "integer",
                nullable: true,
                schema: "data");

            migrationBuilder.CreateTable(
                name: "concept_systems",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    collection_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_concept_systems", x => x.id);
                    table.ForeignKey(
                        name: "fk_concept_systems_collections_collection_id",
                        column: x => x.collection_id,
                        principalTable: "collections",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_entries_concept_system_id",
                table: "entries",
                column: "concept_system_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_concept_systems_collection_id",
                table: "concept_systems",
                column: "collection_id",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_entries_concept_systems_concept_system_id",
                table: "entries",
                schema: "data");

            migrationBuilder.DropTable(
                name: "concept_systems",
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
    }
}
