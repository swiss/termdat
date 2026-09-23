using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddLanguageFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IsoCode",
                table: "LanguageTranslation",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "",
                schema: "data");

            migrationBuilder.AddColumn<bool>(
                name: "IsUiLanguage",
                table: "Language",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "IsoCode",
                table: "Language",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsoCode",
                table: "LanguageTranslation",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "IsUiLanguage",
                table: "Language",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "IsoCode",
                table: "Language",
                schema: "data");
        }
    }
}
