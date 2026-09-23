using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddedObsoleteFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_obsolete",
                table: "entry_languages",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                schema: "data");

            migrationBuilder.AddColumn<bool>(
                name: "is_rare",
                table: "entry_languages",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                schema: "data");

            migrationBuilder.AddColumn<bool>(
                name: "should_avoid",
                table: "entry_languages",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                schema: "data");

            migrationBuilder.AddColumn<bool>(
                name: "is_obsolete",
                table: "collections",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_obsolete",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "is_rare",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "should_avoid",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "is_obsolete",
                table: "collections",
                schema: "data");
        }
    }
}
