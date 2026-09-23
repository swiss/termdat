using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddedTermdatSubjectsAndDomains : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subjects",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subjects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "domains",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domains", x => x.id);
                    table.ForeignKey(
                        name: "fk_domains_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entry_subject",
                schema: "data",
                columns: table => new
                {
                    entries_id = table.Column<int>(type: "integer", nullable: false),
                    subjects_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entry_subject", x => new { x.entries_id, x.subjects_id });
                    table.ForeignKey(
                        name: "fk_entry_subject_entries_entries_id",
                        column: x => x.entries_id,
                        principalTable: "entries",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_entry_subject_subjects_subjects_id",
                        column: x => x.subjects_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subject_translations",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    language_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subject_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_subject_translations_languages_language_id",
                        column: x => x.language_id,
                        principalTable: "languages",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_subject_translations_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
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
                    language_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    domain_id = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "ix_domains_subject_id",
                table: "domains",
                column: "subject_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_entry_subject_subjects_id",
                table: "entry_subject",
                column: "subjects_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_subject_translations_language_id",
                table: "subject_translations",
                column: "language_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_subject_translations_subject_id",
                table: "subject_translations",
                column: "subject_id",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_entry",
                schema: "data");

            migrationBuilder.DropTable(
                name: "domain_translations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "entry_subject",
                schema: "data");

            migrationBuilder.DropTable(
                name: "subject_translations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "domains",
                schema: "data");

            migrationBuilder.DropTable(
                name: "subjects",
                schema: "data");
        }
    }
}
