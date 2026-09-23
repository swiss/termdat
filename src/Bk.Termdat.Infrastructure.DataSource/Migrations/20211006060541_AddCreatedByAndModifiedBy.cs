using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddCreatedByAndModifiedBy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "entries",
                type: "text",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "entries",
                type: "text",
                nullable: true,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                table: "entries",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "entries",
                schema: "data");
        }
    }
}
