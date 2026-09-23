using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class CleanUserAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "user_accounts",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "organisation_unit",
                table: "user_accounts",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "user_accounts",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "user_name",
                table: "user_accounts",
                newName: "email",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "email",
                table: "user_accounts",
                newName: "user_name",
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "user_accounts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "organisation_unit",
                table: "user_accounts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "user_accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                schema: "data");
        }
    }
}
