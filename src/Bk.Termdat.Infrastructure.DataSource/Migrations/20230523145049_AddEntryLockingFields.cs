using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddEntryLockingFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "locked",
                table: "entries",
                type: "timestamp with time zone",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "locked_by",
                table: "entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "locked",
                table: "entries",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "locked_by",
                table: "entries",
                schema: "data");
        }
    }
}
