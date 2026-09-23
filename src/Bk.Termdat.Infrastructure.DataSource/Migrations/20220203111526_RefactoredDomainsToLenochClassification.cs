using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class RefactoredDomainsToLenochClassification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_entry",
                schema: "data");

            migrationBuilder.DropTable(
                name: "domain_translations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "domains",
                schema: "data");

            migrationBuilder.CreateTable(
                name: "lenoch_classifications",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lenoch_classifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entry_lenoch_classification",
                schema: "data",
                columns: table => new
                {
                    entries_id = table.Column<int>(type: "integer", nullable: false),
                    lenoch_classifications_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entry_lenoch_classification",
                        x => new { x.entries_id, x.lenoch_classifications_id });
                    table.ForeignKey(
                        name: "fk_entry_lenoch_classification_entries_entries_id",
                        column: x => x.entries_id,
                        principalTable: "entries",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_entry_lenoch_classification_lenoch_classifications_lenoch_c",
                        column: x => x.lenoch_classifications_id,
                        principalTable: "lenoch_classifications",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lenoch_classification_translations",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    language_id = table.Column<int>(type: "integer", nullable: false),
                    lenoch_classification_id = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lenoch_classification_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_lenoch_classification_translations_languages_language_id",
                        column: x => x.language_id,
                        principalTable: "languages",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lenoch_classification_translations_lenoch_classifications_l",
                        column: x => x.lenoch_classification_id,
                        principalTable: "lenoch_classifications",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_entry_lenoch_classification_lenoch_classifications_id",
                table: "entry_lenoch_classification",
                column: "lenoch_classifications_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_lenoch_classification_translations_language_id",
                table: "lenoch_classification_translations",
                column: "language_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_lenoch_classification_translations_lenoch_classification_id",
                table: "lenoch_classification_translations",
                column: "lenoch_classification_id",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entry_lenoch_classification",
                schema: "data");

            migrationBuilder.DropTable(
                name: "lenoch_classification_translations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "lenoch_classifications",
                schema: "data");

            migrationBuilder.CreateTable(
                name: "domains",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domains", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "domain_entry",
                schema: "data",
                columns: table => new
                {
                    domains_id = table.Column<int>(type: "integer", nullable: false),
                    entries_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domain_entry", x => new { x.domains_id, x.entries_id });
                    table.ForeignKey(
                        name: "fk_domain_entry_domains_domains_id",
                        column: x => x.domains_id,
                        principalTable: "domains",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_domain_entry_entries_entries_id",
                        column: x => x.entries_id,
                        principalTable: "entries",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "domain_translations",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    domain_id = table.Column<int>(type: "integer", nullable: false),
                    language_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domain_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_domain_translations_domains_domain_id",
                        column: x => x.domain_id,
                        principalTable: "domains",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_domain_translations_languages_language_id",
                        column: x => x.language_id,
                        principalTable: "languages",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_domain_entry_entries_id",
                table: "domain_entry",
                column: "entries_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_domain_translations_domain_id",
                table: "domain_translations",
                column: "domain_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_domain_translations_language_id",
                table: "domain_translations",
                column: "language_id",
                schema: "data");
        }
    }
}
