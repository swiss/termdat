using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalSystemIdField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_system_id",
                table: "entry_languages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "external_system_id",
                table: "entries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                schema: "data");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "external_system_id",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "external_system_id",
                table: "entries",
                schema: "data");
        }
    }
}
