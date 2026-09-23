using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddDescriptionToAdminInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "admin_infos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_admin_infos_code",
                table: "admin_infos",
                column: "code",
                unique: true,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_admin_infos_code",
                table: "admin_infos",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "description",
                table: "admin_infos",
                schema: "data");
        }
    }
}
