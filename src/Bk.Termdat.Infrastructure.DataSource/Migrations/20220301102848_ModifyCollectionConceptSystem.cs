using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class ModifyCollectionConceptSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_concept_systems_collections_collection_id",
                table: "concept_systems",
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_concept_systems_collections_collection_id",
                table: "concept_systems",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_concept_systems_collections_collection_id",
                table: "concept_systems",
                column: "collection_id",
                principalSchema: "data",
                principalTable: "collections",
                principalColumn: "id",
                schema: "data");
        }
    }
}
