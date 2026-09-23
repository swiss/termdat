using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class RenameAdminUnitsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_admin_info_entry_admin_info_admin_infos_id",
                table: "admin_info_entry",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_admin_info",
                table: "admin_info",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "admin_info",
                newName: "admin_infos",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_admin_infos",
                table: "admin_infos",
                column: "id",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_admin_info_entry_admin_infos_admin_infos_id",
                table: "admin_info_entry",
                column: "admin_infos_id",
                principalTable: "admin_infos",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_admin_info_entry_admin_infos_admin_infos_id",
                table: "admin_info_entry",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_admin_infos",
                table: "admin_infos",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "admin_infos",
                newName: "admin_info",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_admin_info",
                table: "admin_info",
                column: "id",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_admin_info_entry_admin_info_admin_infos_id",
                table: "admin_info_entry",
                column: "admin_infos_id",
                principalTable: "admin_info",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");
        }
    }
}
