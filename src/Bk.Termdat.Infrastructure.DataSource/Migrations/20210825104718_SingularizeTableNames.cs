using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class SingularizeTableNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionTranslations_Collections_CollectionId",
                table: "CollectionTranslations",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionTranslations_Languages_LanguageId",
                table: "CollectionTranslations",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_Entries_Collections_CollectionId",
                table: "Entries",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryTranslations_Entries_EntryId",
                table: "EntryTranslations",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryTranslations_Languages_LanguageId",
                table: "EntryTranslations",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_LanguageTranslations_Languages_LanguageId",
                table: "LanguageTranslations",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LanguageTranslations",
                table: "LanguageTranslations",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Languages",
                table: "Languages",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntryTranslations",
                table: "EntryTranslations",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Entries",
                table: "Entries",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CollectionTranslations",
                table: "CollectionTranslations",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Collections",
                table: "Collections",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "LanguageTranslations",
                newName: "LanguageTranslation",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "Languages",
                newName: "Language",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "EntryTranslations",
                newName: "EntryTranslation",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "Entries",
                newName: "Entry",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "CollectionTranslations",
                newName: "CollectionTranslation",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "Collections",
                newName: "Collection",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_LanguageTranslations_LanguageId",
                table: "LanguageTranslation",
                newName: "IX_LanguageTranslation_LanguageId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_EntryTranslations_LanguageId",
                table: "EntryTranslation",
                newName: "IX_EntryTranslation_LanguageId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_EntryTranslations_EntryId",
                table: "EntryTranslation",
                newName: "IX_EntryTranslation_EntryId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_Entries_CollectionId",
                table: "Entry",
                newName: "IX_Entry_CollectionId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_CollectionTranslations_LanguageId",
                table: "CollectionTranslation",
                newName: "IX_CollectionTranslation_LanguageId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_CollectionTranslations_CollectionId",
                table: "CollectionTranslation",
                newName: "IX_CollectionTranslation_CollectionId",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LanguageTranslation",
                table: "LanguageTranslation",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Language",
                table: "Language",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntryTranslation",
                table: "EntryTranslation",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Entry",
                table: "Entry",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CollectionTranslation",
                table: "CollectionTranslation",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Collection",
                table: "Collection",
                column: "Id",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionTranslation_Collection_CollectionId",
                table: "CollectionTranslation",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionTranslation_Language_LanguageId",
                table: "CollectionTranslation",
                column: "LanguageId",
                principalTable: "Language",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_Entry_Collection_CollectionId",
                table: "Entry",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
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

            migrationBuilder.AddForeignKey(
                name: "FK_LanguageTranslation_Language_LanguageId",
                table: "LanguageTranslation",
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
                name: "FK_CollectionTranslation_Collection_CollectionId",
                table: "CollectionTranslation",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionTranslation_Language_LanguageId",
                table: "CollectionTranslation",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_Entry_Collection_CollectionId",
                table: "Entry",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryTranslation_Entry_EntryId",
                table: "EntryTranslation",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryTranslation_Language_LanguageId",
                table: "EntryTranslation",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_LanguageTranslation_Language_LanguageId",
                table: "LanguageTranslation",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LanguageTranslation",
                table: "LanguageTranslation",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Language",
                table: "Language",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntryTranslation",
                table: "EntryTranslation",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Entry",
                table: "Entry",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CollectionTranslation",
                table: "CollectionTranslation",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Collection",
                table: "Collection",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "LanguageTranslation",
                newName: "LanguageTranslations",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "Language",
                newName: "Languages",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "EntryTranslation",
                newName: "EntryTranslations",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "Entry",
                newName: "Entries",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "CollectionTranslation",
                newName: "CollectionTranslations",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "Collection",
                newName: "Collections",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_LanguageTranslation_LanguageId",
                table: "LanguageTranslations",
                newName: "IX_LanguageTranslations_LanguageId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_EntryTranslation_LanguageId",
                table: "EntryTranslations",
                newName: "IX_EntryTranslations_LanguageId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_EntryTranslation_EntryId",
                table: "EntryTranslations",
                newName: "IX_EntryTranslations_EntryId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_Entry_CollectionId",
                table: "Entries",
                newName: "IX_Entries_CollectionId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_CollectionTranslation_LanguageId",
                table: "CollectionTranslations",
                newName: "IX_CollectionTranslations_LanguageId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_CollectionTranslation_CollectionId",
                table: "CollectionTranslations",
                newName: "IX_CollectionTranslations_CollectionId",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LanguageTranslations",
                table: "LanguageTranslations",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Languages",
                table: "Languages",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntryTranslations",
                table: "EntryTranslations",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Entries",
                table: "Entries",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CollectionTranslations",
                table: "CollectionTranslations",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Collections",
                table: "Collections",
                column: "Id",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionTranslations_Collections_CollectionId",
                table: "CollectionTranslations",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionTranslations_Languages_LanguageId",
                table: "CollectionTranslations",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_Collections_CollectionId",
                table: "Entries",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryTranslations_Entries_EntryId",
                table: "EntryTranslations",
                column: "EntryId",
                principalTable: "Entries",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryTranslations_Languages_LanguageId",
                table: "EntryTranslations",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_LanguageTranslations_Languages_LanguageId",
                table: "LanguageTranslations",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");
        }
    }
}
