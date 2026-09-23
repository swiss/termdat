using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class ExtendEntryLanguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "context",
                table: "entry_languages",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "entry_languages",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "metadata",
                table: "entry_languages",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "entry_languages",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "reference",
                table: "entry_languages",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "entry_languages",
                type: "text",
                nullable: true,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "context",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "country",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "metadata",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "note",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "reference",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "source",
                table: "entry_languages",
                schema: "data");
        }
    }
}
