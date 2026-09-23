using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddEntryLanguageSourceFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "source",
                table: "entry_languages",
                newName: "terminus_source",
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "abbreviation_source",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "context_source",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "definition_source",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "metadata_source",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "name_source",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "note_source",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "phraseology_source",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "abbreviation_source",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "context_source",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "definition_source",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "metadata_source",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "name_source",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "note_source",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "phraseology_source",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "terminus_source",
                table: "entry_languages",
                newName: "source",
                schema: "data");
        }
    }
}
