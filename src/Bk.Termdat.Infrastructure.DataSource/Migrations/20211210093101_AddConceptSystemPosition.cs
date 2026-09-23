using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddConceptSystemPosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_concept_systems_collections_collection_id",
                table: "concept_systems",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_entries_concept_systems_concept_system_id",
                table: "entries",
                schema: "data");

            migrationBuilder.DropIndex(
                name: "ix_concept_systems_collection_id",
                table: "concept_systems",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "value",
                table: "concept_systems",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "concept_system_id",
                table: "entries",
                newName: "concept_system_position_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_entries_concept_system_id",
                table: "entries",
                newName: "ix_entries_concept_system_position_id",
                schema: "data");

            migrationBuilder.CreateTable(
                name: "concept_system_positions",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<string>(type: "text", nullable: true),
                    concept_system_id = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_concept_system_positions", x => x.id);
                    table.ForeignKey(
                        name: "fk_concept_system_positions_concept_systems_concept_system_id",
                        column: x => x.concept_system_id,
                        principalTable: "concept_systems",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_concept_systems_collection_id",
                table: "concept_systems",
                column: "collection_id",
                unique: true,
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_concept_system_positions_concept_system_id",
                table: "concept_system_positions",
                column: "concept_system_id",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_concept_systems_collections_collection_id",
                table: "concept_systems",
                column: "collection_id",
                principalTable: "collections",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_entries_concept_system_positions_concept_system_position_id",
                table: "entries",
                column: "concept_system_position_id",
                principalTable: "concept_system_positions",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_concept_systems_collections_collection_id",
                table: "concept_systems",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_entries_concept_system_positions_concept_system_position_id",
                table: "entries",
                schema: "data");

            migrationBuilder.DropTable(
                name: "concept_system_positions",
                schema: "data");

            migrationBuilder.DropIndex(
                name: "ix_concept_systems_collection_id",
                table: "concept_systems",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "concept_system_position_id",
                table: "entries",
                newName: "concept_system_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_entries_concept_system_position_id",
                table: "entries",
                newName: "ix_entries_concept_system_id",
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "value",
                table: "concept_systems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_concept_systems_collection_id",
                table: "concept_systems",
                column: "collection_id",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_concept_systems_collections_collection_id",
                table: "concept_systems",
                column: "collection_id",
                principalTable: "collections",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
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
