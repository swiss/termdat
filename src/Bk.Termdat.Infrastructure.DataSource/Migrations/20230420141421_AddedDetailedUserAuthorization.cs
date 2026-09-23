using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddedDetailedUserAuthorization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "user_accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_action",
                table: "user_accounts",
                type: "timestamp with time zone",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_login",
                table: "user_accounts",
                type: "timestamp with time zone",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "created",
                table: "user_accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "user_accounts",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "modified",
                table: "user_accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "user_accounts",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.CreateTable(
                name: "collection_user_account",
                schema: "data",
                columns: table => new
                {
                    authorized_collections_id = table.Column<int>(type: "integer", nullable: false),
                    user_accounts_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collection_user_account",
                        x => new { x.authorized_collections_id, x.user_accounts_id });
                    table.ForeignKey(
                        name: "fk_collection_user_account_collections_authorized_collections_",
                        column: x => x.authorized_collections_id,
                        principalTable: "collections",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_collection_user_account_user_accounts_user_accounts_id",
                        column: x => x.user_accounts_id,
                        principalTable: "user_accounts",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "glossary_user_account",
                schema: "data",
                columns: table => new
                {
                    authorized_glossaries_id = table.Column<int>(type: "integer", nullable: false),
                    user_accounts_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_glossary_user_account",
                        x => new { x.authorized_glossaries_id, x.user_accounts_id });
                    table.ForeignKey(
                        name: "fk_glossary_user_account_glossaries_authorized_glossaries_id",
                        column: x => x.authorized_glossaries_id,
                        principalTable: "glossaries",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_glossary_user_account_user_accounts_user_accounts_id",
                        column: x => x.user_accounts_id,
                        principalTable: "user_accounts",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_collection_user_account_user_accounts_id",
                table: "collection_user_account",
                column: "user_accounts_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_glossary_user_account_user_accounts_id",
                table: "glossary_user_account",
                column: "user_accounts_id",
                schema: "data");

            // Add values for existing entries so default values can be removed in second migration
            migrationBuilder.Sql(
                $"UPDATE data.user_accounts SET created = NOW(), modified = NOW(), created_by = 'System', modified_by = 'System'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "collection_user_account",
                schema: "data");

            migrationBuilder.DropTable(
                name: "glossary_user_account",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created",
                table: "user_accounts",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "user_accounts",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "last_action",
                table: "user_accounts",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "last_login",
                table: "user_accounts",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified",
                table: "user_accounts",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "user_accounts",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "role",
                table: "user_accounts",
                schema: "data");
        }
    }
}
