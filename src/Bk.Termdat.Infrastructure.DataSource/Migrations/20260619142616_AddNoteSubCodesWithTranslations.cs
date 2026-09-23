using System;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteSubCodesWithTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "note_sub_code_id",
                schema: "data",
                table: "user_accounts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "note_sub_code_id",
                schema: "data",
                table: "entries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "note_sub_codes",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_note_sub_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "note_sub_code_translations",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    language_id = table.Column<int>(type: "integer", nullable: false),
                    note_sub_code_id = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_note_sub_code_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_note_sub_code_translations_languages_language_id",
                        column: x => x.language_id,
                        principalSchema: "data",
                        principalTable: "languages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_note_sub_code_translations_note_sub_codes_note_sub_code_id",
                        column: x => x.note_sub_code_id,
                        principalSchema: "data",
                        principalTable: "note_sub_codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_accounts_note_sub_code_id",
                schema: "data",
                table: "user_accounts",
                column: "note_sub_code_id");

            migrationBuilder.CreateIndex(
                name: "ix_entries_note_sub_code_id",
                schema: "data",
                table: "entries",
                column: "note_sub_code_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_sub_code_translations_language_id",
                schema: "data",
                table: "note_sub_code_translations",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_sub_code_translations_note_sub_code_id",
                schema: "data",
                table: "note_sub_code_translations",
                column: "note_sub_code_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_sub_codes_code",
                schema: "data",
                table: "note_sub_codes",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_entries_note_sub_codes_note_sub_code_id",
                schema: "data",
                table: "entries",
                column: "note_sub_code_id",
                principalSchema: "data",
                principalTable: "note_sub_codes",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_accounts_note_sub_codes_note_sub_code_id",
                schema: "data",
                table: "user_accounts",
                column: "note_sub_code_id",
                principalSchema: "data",
                principalTable: "note_sub_codes",
                principalColumn: "id");

            migrationBuilder.Sql(@"INSERT INTO data.note_sub_codes(id, code, created, created_by, modified, modified_by) 
                VALUES 
                (1, 'ANT', now(), 'SYSTEM', now(), 'SYSTEM'), 
                (2, 'DOM', now(), 'SYSTEM', now(), 'SYSTEM'), 
                (3, 'EXP', now(), 'SYSTEM', now(), 'SYSTEM'), 
                (4, 'GEN', now(), 'SYSTEM', now(), 'SYSTEM'), 
                (5, 'GRM', now(), 'SYSTEM', now(), 'SYSTEM'), 
                (6, 'HIS', now(), 'SYSTEM', now(), 'SYSTEM'), 
                (7, 'LIN', now(), 'SYSTEM', now(), 'SYSTEM'), 
                (8, 'REG', now(), 'SYSTEM', now(), 'SYSTEM'), 
                (9, 'SPE', now(), 'SYSTEM', now(), 'SYSTEM'), 
                (10, 'STA', now(), 'SYSTEM', now(), 'SYSTEM'), 
                (11, 'SYS', now(), 'SYSTEM', now(), 'SYSTEM'), 
                (12, 'USG', now(), 'SYSTEM', now(), 'SYSTEM');");

            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (1, 2, 'Antonym'), (1, 6, 'Antonyme'), (1, 7, 'Antonimo'), (1, 8, 'Antonim'), (1, 3, 'antonym');");
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (2, 2, 'Sachgebiet'), (2, 6, 'Domaine'), (2, 7, 'Dominio'), (2, 8, 'Domena'), (2, 3, 'domain');");
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (3, 2, 'Erläuterungen'), (3, 6, 'Explications'), (3, 7, 'Note esplicative'), (3, 8, 'Explicaziuns'), (3, 3, 'explanatory note');");
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (4, 2, 'Oberbegriff'), (4, 6, 'Terme générique'), (4, 7, 'Concetto sovraordinato'), (4, 8, 'Noziun generala'), (4, 3, 'broader term');");
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (5, 2, 'Grammatik'), (5, 6, 'Grammaire'), (5, 7, 'Grammatica'), (5, 8, 'Grammatica'), (5, 3, 'grammar');");
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (6, 2, 'Geschichte'), (6, 6, 'Historique'), (6, 7, 'Note storiche'), (6, 8, 'Istorgia'), (6, 3, 'history');");
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (7, 2, 'Sprache'), (7, 6, 'Langue'), (7, 7, 'Lingua'), (7, 8, 'Lingua'), (7, 3, 'language');");
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (8, 2, 'Region'), (8, 6, 'Région'), (8, 7, 'Regione'), (8, 8, 'Regiun'), (8, 3, 'region');");
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (9, 2, 'Unterbegriff'), (9, 6, 'Terme spécifique'), (9, 7, 'Concetto subordinato'), (9, 8, 'Noziun subordinada'), (9, 3, 'narrower term');");
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (10, 2, 'Status'), (10, 6, 'Statut'), (10, 7, 'Stato'), (10, 8, 'Status'), (10, 3, 'status');");
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (11, 2, 'Klassifikationssystem'), (11, 6, 'Système de classification'), (11, 7, 'Classificazione'), (11, 8, 'Classificaziun'), (11, 3, 'classification system');");
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (12, 2, 'Verwendungshinweis'), (12, 6, 'Usage'), (12, 7, 'Note d’uso'), (12, 8, 'Indicaziun da diever'), (12, 3, 'usage note');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_entries_note_sub_codes_note_sub_code_id",
                schema: "data",
                table: "entries");

            migrationBuilder.DropForeignKey(
                name: "fk_user_accounts_note_sub_codes_note_sub_code_id",
                schema: "data",
                table: "user_accounts");

            migrationBuilder.DropTable(
                name: "note_sub_code_translations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "note_sub_codes",
                schema: "data");

            migrationBuilder.DropIndex(
                name: "ix_user_accounts_note_sub_code_id",
                schema: "data",
                table: "user_accounts");

            migrationBuilder.DropIndex(
                name: "ix_entries_note_sub_code_id",
                schema: "data",
                table: "entries");

            migrationBuilder.DropColumn(
                name: "note_sub_code_id",
                schema: "data",
                table: "user_accounts");

            migrationBuilder.DropColumn(
                name: "note_sub_code_id",
                schema: "data",
                table: "entries");
        }
    }
}
