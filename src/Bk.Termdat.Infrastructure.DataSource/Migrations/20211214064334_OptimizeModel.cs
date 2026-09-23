using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class OptimizeModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "value",
                table: "offices",
                newName: "code",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "domains",
                newName: "code",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "collections",
                newName: "code",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "classifications",
                newName: "code",
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "offices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "offices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "languages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "languages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "entries",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "now()",
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "entries",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "now()",
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "domains",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "domains",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "concept_systems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "concept_systems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "collections",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "collections",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "classifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "classifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "code",
                table: "offices",
                newName: "value",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "domains",
                newName: "value",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "collections",
                newName: "value",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "classifications",
                newName: "value",
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "offices",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "offices",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "languages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "languages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "entries",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "entries",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "domains",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "domains",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "concept_systems",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "concept_systems",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "collections",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "collections",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "classifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "classifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");
        }
    }
}
