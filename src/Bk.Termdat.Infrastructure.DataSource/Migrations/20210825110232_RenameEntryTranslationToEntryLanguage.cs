using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class RenameEntryTranslationToEntryLanguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntryTranslation_Entry_EntryId",
                table: "EntryTranslation",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryTranslation_Language_LanguageId",
                table: "EntryTranslation",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntryTranslation",
                table: "EntryTranslation",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "EntryTranslation",
                newName: "EntryLanguage",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_EntryTranslation_LanguageId",
                table: "EntryLanguage",
                newName: "IX_EntryLanguage_LanguageId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_EntryTranslation_EntryId",
                table: "EntryLanguage",
                newName: "IX_EntryLanguage_EntryId",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntryLanguage",
                table: "EntryLanguage",
                column: "Id",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryLanguage_Entry_EntryId",
                table: "EntryLanguage",
                column: "EntryId",
                principalTable: "Entry",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryLanguage_Language_LanguageId",
                table: "EntryLanguage",
                column: "LanguageId",
                principalTable: "Language",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntryLanguage_Entry_EntryId",
                table: "EntryLanguage",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryLanguage_Language_LanguageId",
                table: "EntryLanguage",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntryLanguage",
                table: "EntryLanguage",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "EntryLanguage",
                newName: "EntryTranslation",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_EntryLanguage_LanguageId",
                table: "EntryTranslation",
                newName: "IX_EntryTranslation_LanguageId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_EntryLanguage_EntryId",
                table: "EntryTranslation",
                newName: "IX_EntryTranslation_EntryId",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntryTranslation",
                table: "EntryTranslation",
                column: "Id",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryTranslation_Entry_EntryId",
                table: "EntryTranslation",
                column: "EntryId",
                principalTable: "Entry",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryTranslation_Language_LanguageId",
                table: "EntryTranslation",
                column: "LanguageId",
                principalTable: "Language",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");
        }
    }
}
