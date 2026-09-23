using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddClassification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "classification_id",
                table: "collections",
                type: "integer",
                nullable: true,
                schema: "data");

            migrationBuilder.CreateTable(
                name: "classifications",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_classifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "classification_translations",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    language_id = table.Column<int>(type: "integer", nullable: false),
                    classification_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_classification_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_classification_translations_classifications_classification_",
                        column: x => x.classification_id,
                        principalTable: "classifications",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_classification_translations_languages_language_id",
                        column: x => x.language_id,
                        principalTable: "languages",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_collections_classification_id",
                table: "collections",
                column: "classification_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_classification_translations_classification_id",
                table: "classification_translations",
                column: "classification_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_classification_translations_language_id",
                table: "classification_translations",
                column: "language_id",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_collections_classifications_classification_id",
                table: "collections",
                column: "classification_id",
                principalTable: "classifications",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_collections_classifications_classification_id",
                table: "collections",
                schema: "data");

            migrationBuilder.DropTable(
                name: "classification_translations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "classifications",
                schema: "data");

            migrationBuilder.DropIndex(
                name: "ix_collections_classification_id",
                table: "collections",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "classification_id",
                table: "collections",
                schema: "data");
        }
    }
}
