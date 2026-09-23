using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    /// <inheritdoc />
    public partial class CollectionGlossarDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "data",
                table: "glossaries",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "data",
                table: "collections",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                schema: "data",
                table: "glossaries");

            migrationBuilder.DropColumn(
                name: "description",
                schema: "data",
                table: "collections");
        }
    }
}
