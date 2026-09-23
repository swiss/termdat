using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddPictureEntityTag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "pictures",
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "entity_tag",
                table: "pictures",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "entity_tag",
                table: "pictures",
                schema: "data");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "pictures",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                schema: "data");
        }
    }
}
