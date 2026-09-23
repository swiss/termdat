using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddGlossary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "glossaries",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_glossaries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entry_glossary",
                schema: "data",
                columns: table => new
                {
                    entries_id = table.Column<int>(type: "integer", nullable: false),
                    glossaries_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entry_glossary", x => new { x.entries_id, x.glossaries_id });
                    table.ForeignKey(
                        name: "fk_entry_glossary_entries_entries_id",
                        column: x => x.entries_id,
                        principalTable: "entries",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_entry_glossary_glossaries_glossaries_id",
                        column: x => x.glossaries_id,
                        principalTable: "glossaries",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "glossary_translations",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    language_id = table.Column<int>(type: "integer", nullable: false),
                    glossary_id = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_glossary_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_glossary_translations_glossaries_glossary_id",
                        column: x => x.glossary_id,
                        principalTable: "glossaries",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_glossary_translations_languages_language_id",
                        column: x => x.language_id,
                        principalTable: "languages",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_entry_glossary_glossaries_id",
                table: "entry_glossary",
                column: "glossaries_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_glossary_translations_glossary_id",
                table: "glossary_translations",
                column: "glossary_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_glossary_translations_language_id",
                table: "glossary_translations",
                column: "language_id",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entry_glossary",
                schema: "data");

            migrationBuilder.DropTable(
                name: "glossary_translations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "glossaries",
                schema: "data");
        }
    }
}
