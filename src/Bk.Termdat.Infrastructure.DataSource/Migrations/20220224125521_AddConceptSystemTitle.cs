using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddConceptSystemTitle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "offices",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "offices",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "languages",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "languages",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "glossaries",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "glossaries",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "entries",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "entries",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "domains",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "domains",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "concept_systems",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "concept_systems",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "concept_systems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "collections",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "collections",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "classifications",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "classifications",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "title",
                table: "concept_systems",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "offices",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "offices",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "languages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "languages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "glossaries",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "glossaries",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "entries",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "entries",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "domains",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "domains",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "concept_systems",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "concept_systems",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "collections",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "collections",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "classifications",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "classifications",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                schema: "data");
        }
    }
}
