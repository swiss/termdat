using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddPropertiesMaxLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "source",
                table: "picture_translations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "picture_translations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "office_translations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "source",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "reference",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "note",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "metadata",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "country",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "context",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "entries",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "admin_info",
                table: "entries",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "domain_translations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "collection_translations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "classification_translations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "source",
                table: "picture_translations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "picture_translations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "office_translations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "source",
                table: "entry_languages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "reference",
                table: "entry_languages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "note",
                table: "entry_languages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "metadata",
                table: "entry_languages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "country",
                table: "entry_languages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "context",
                table: "entry_languages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "entries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "entries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "entries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "admin_info",
                table: "entries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "domain_translations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "collection_translations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "classification_translations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                schema: "data");
        }
    }
}
