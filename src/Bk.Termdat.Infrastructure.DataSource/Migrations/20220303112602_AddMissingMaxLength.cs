using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddMissingMaxLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "value",
                table: "concept_system_positions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "documentation",
                table: "collections",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "value",
                table: "concept_system_positions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "documentation",
                table: "collections",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                schema: "data");
        }
    }
}
