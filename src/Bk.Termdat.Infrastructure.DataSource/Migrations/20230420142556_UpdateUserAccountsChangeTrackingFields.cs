using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class UpdateUserAccountsChangeTrackingFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "user_accounts",
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
                table: "user_accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "user_accounts",
                oldDefaultValue: true,
                defaultValue: null,
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "user_accounts",
                oldDefaultValue: true,
                defaultValue: null,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "modified_by",
                table: "user_accounts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "user_accounts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created",
                table: "user_accounts",
                oldDefaultValue: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified",
                table: "user_accounts",
                oldDefaultValue: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");
        }
    }
}
