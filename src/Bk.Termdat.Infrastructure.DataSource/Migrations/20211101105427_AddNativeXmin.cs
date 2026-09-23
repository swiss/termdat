using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddNativeXmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "user_accounts",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "pictures",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "picture_translations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "offices",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "office_translations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "languages",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "language_translations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "entry_languages",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "entries",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "domains",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "domain_translations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "concept_systems",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "collections",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "collection_translations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "classifications",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "classification_translations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "user_accounts",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "pictures",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "picture_translations",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "offices",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "office_translations",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "language_translations",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "entries",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "domains",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "domain_translations",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "concept_systems",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "collections",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "collection_translations",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "classifications",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "classification_translations",
                schema: "data");
        }
    }
}
