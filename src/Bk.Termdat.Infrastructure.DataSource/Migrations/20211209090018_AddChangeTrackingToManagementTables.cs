using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddChangeTrackingToManagementTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created",
                table: "offices",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "offices",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "modified",
                table: "offices",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "offices",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "created",
                table: "languages",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "languages",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "modified",
                table: "languages",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "languages",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "created",
                table: "domains",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "domains",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "modified",
                table: "domains",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "domains",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "created",
                table: "concept_systems",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "concept_systems",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "modified",
                table: "concept_systems",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "concept_systems",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "collections",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                schema: "data");

            migrationBuilder.AddColumn<bool>(
                name: "active_api",
                table: "collections",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "created",
                table: "collections",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "collections",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "documentation",
                table: "collections",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "modified",
                table: "collections",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "collections",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "created",
                table: "classifications",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "classifications",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "modified",
                table: "classifications",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "classifications",
                type: "text",
                nullable: true,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created",
                table: "offices",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "offices",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "offices",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "offices",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created",
                table: "languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created",
                table: "domains",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "domains",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "domains",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "domains",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created",
                table: "concept_systems",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "concept_systems",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "concept_systems",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "concept_systems",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "active",
                table: "collections",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "active_api",
                table: "collections",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created",
                table: "collections",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "collections",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "documentation",
                table: "collections",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "collections",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "collections",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created",
                table: "classifications",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "classifications",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "classifications",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "classifications",
                schema: "data");
        }
    }
}
